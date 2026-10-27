---
name: Albatross.Logging.Daemon
status: active
created: 2026-10-08T08:24:27-04:00
updated: 2026-10-27T10:20:00-04:00
---

## Business Requirements

Apps run on multiple servers and write rolling log files to a central log location on each server. These logs hold a lot of information, but nobody can read them all. The daemon reads the logs for developers and answers four questions, in priority order:

1. **What is going wrong?** *(first priority)* The daemon finds errors and reports a summary of them. An app can produce a burst of errors in a short time, so the summary has to work out *what* the errors are: their distinct causes, how many of each, and where they come from. A count such as "App X logged 500 errors" does not meet this requirement.
2. **What is being used?** *(second priority)* The daemon records usage statistics for each app and endpoint over time, so usage patterns can be queried later. This shows whether an endpoint is used at all. Lack of usage is a finding in itself: an endpoint with no log activity is either unused or not being logged. Usage is not sent as a report. It is stored for querying.
3. **Is logging sufficient?** *(third priority)* Apps must meet minimum logging requirements. The daemon checks each app's logs against those requirements and reports where they fall short, so the app can be fixed.
4. **How is it performing?** *(fourth priority)* The daemon pulls performance measurements out of the logs, such as how long operations take, so they can be benchmarked over time. What counts as a measurement is likely different for each app, so this is an app-specific capability rather than a general one.

Developers are the primary readers. Reports can include technical detail such as exception types, source context, and representative messages.

## Technical Design

The daemon is a long-running service built on Albatross.Hosting. It can run as a Windows service and uses Albatross.Messaging for its command bus.

It finds app log folders under a log root and through configured entries, and reads new lines as they are appended. Each file's format is detected from a shared library of format definitions, and its lines are parsed into entries using that format. A single parsed entry then feeds four paths:

- **Error path:** Error and Fatal entries are collected over a short batching window. When the window closes, a rules-based stage groups them into distinct errors and counts them. An optional LLM stage adds a short narrative, and the summary goes out to the configured delivery channels.
- **Usage path:** entries that record endpoint activity are added to usage statistics in the database, grouped by app, endpoint, and time bucket. Nothing is delivered. The statistics are there to be queried.
- **Sufficiency path:** each app's logs are checked against the minimum logging requirements, and any app that falls short is reported.
- **Performance path:** for apps that have it configured, app-specific rules pull timing measurements out of entries and store them as benchmark statistics in the database.

Format definitions, processing state, and all output are stored in one database, which can be SQL Server or PostgreSQL. Data access uses one shared Albatross.EFCore entity model, with a separate database session for each provider. Configuration picks the provider, and each provider has its own migrations.

## Key Design Decisions

### Capabilities are prioritized errors, usage, sufficiency, performance

The capabilities are built and tuned in priority order: error summaries first, usage statistics second, logging sufficiency third, performance benchmarks fourth. When they compete for effort or resources, the higher priority wins.

### Summaries target developers

Reports are written for developers. They keep technical detail: exception types, source context, and sample messages. Content is not simplified for non-technical readers.

### Log files are watched continuously

The daemon follows log files as they grow instead of scanning them in scheduled batches, so errors show up close to when they happen. This means the daemon has to keep track of how far it has read in each file.

### Files are processed in creation order

Each app's log files are processed in the order they were created, so statistics and findings build up in time order. Processing them out of order, or several at once for the same app, was rejected because each file's results depend on what came before it.

### Each file is processed once, and reprocessing overwrites

Each log file is normally processed only once. All output derived from a file (usage statistics, error groups, sufficiency findings, benchmark statistics) is tied to that file, so processing it again replaces its previous output instead of adding to it. This makes reprocessing safe. A file can be run again after a parser fix or a template change without counting anything twice. A file can only be reprocessed while it still exists, which means within the six months before log files are purged.

### Daemon output is permanent data

Everything the daemon produces (usage statistics, error groups, sufficiency findings, benchmark statistics) is kept for good, even after the log files it came from have been purged. A missing source file never causes its output to be deleted. This output is the only long-term record of what the logs said, so the daemon's output is treated as data and the log files are treated as temporary input.

### Error summaries identify distinct errors

A summary breaks errors down into distinct causes, each with a count, a time range, its source, and a representative message. A plain count of errors for each app was rejected, because during a burst the developer needs to know what broke, not just how much logging happened.

### Every error is reported

Every Error or Fatal entry is included in a summary. There is no "already seen" filter. Holding back errors that look like ones seen before was rejected, because a recurring error is still an error a developer needs to know about.

### Errors are batched briefly before sending

Errors are collected over a short window, and each window goes out as one summary. Sending one alert per error was rejected, because a single failure, such as a dependency going down, can produce a flood of errors.

### Only Error and Fatal levels are reported

Error summaries ignore Warning and lower levels. Including warnings was rejected because they are noisy, and that would hide the real errors. This applies only to error reporting. The usage path reads entries at any level.

### Usage is stored, not reported

Usage statistics go to the database for later querying, and no usage report is sent. The value of usage data is in trends over time and in finding endpoints with no activity, and both are better served by queries than by a pushed report.

### Performance benchmarks are app-specific

Performance extraction is set up for each app individually. It is not a general rule applied to every app, because apps don't log timings in a shared format. An app with no performance setup is simply skipped by this path. Like usage, benchmark statistics are stored to be queried, not sent as reports.

### Formats come from a shared definition library

Logs are plain text in a small number of distinct formats, so each format is defined once, given a name, and shared by every app that uses it. Defining a format for each app was rejected, because many apps share the same template. A Serilog format is defined by its output template, the same string developers write in their Serilog configuration. A format from another logging library is defined by a regular expression with named fields, and can say how to get the date from the file name when lines only have the time of day. A line that doesn't match the start of the format continues the previous entry, which keeps stack traces attached. Level names are mapped to one standard set. Embedded JSON is treated as part of the message.

### Each file's format is detected

The daemon finds a file's format by trying the known definitions against the first lines of the file and picking the best match. Configuration can pin an app to a format when detection gets it wrong. A required mapping from each app to its format was rejected, because it is one more list to keep up to date. Detecting per file also handles an app that changes its template partway through its life. A file that matches no format well enough is reported as a logging sufficiency finding.

### Format definitions are versioned

Each change to a format definition creates a new version, and output records which version produced it. That shows which files were parsed with a faulty definition and need reprocessing while their source files still exist.

### Log files are found by discovery plus configuration

The daemon discovers app logs under a log root (one subfolder per app). Configuration can add locations outside that root, exclude discovered ones, and pin an app to a format. This way new apps are picked up with little setup, and odd layouts can still be handled.

### Rules first, LLM optional

A deterministic stage always produces the error summary by grouping and counting entries. An LLM can optionally add a narrative on top of that output, but it is never the only path. Summaries still work, and stay the same every run, when no LLM is configured. Sending raw logs straight to an LLM was rejected because of cost, privacy, and inconsistent output.

### Multiple delivery channels

Error summaries and sufficiency findings can be delivered to files, Slack, and email. Each channel is optional and is configured on its own.

### Everything is kept in one database

Format definitions, configuration, processing state, and all output live in one database. Keeping everything in files was rejected for three reasons. Daemons on several servers need to share the same definitions. The permanent output has to be queryable across all apps. And with one store, only one thing has to be deployed and backed up. The cost is that definitions are no longer files tracked in source control, so they need their own management tool.

### Both databases are first-class

SQL Server and PostgreSQL are equally supported. Neither one is the primary target, and the provider is chosen in configuration. There is one entity model built on Albatross.EFCore, a database session for each provider, and migrations generated separately for each. Any schema, query, or migration change is incomplete until it works on both providers. Supporting only one engine was rejected, because the daemon should run on whichever database a team already operates.

## Open Questions

### Errors

- **Error grouping:** how are two error lines judged to be "the same error"? A likely key is source context plus exception type plus a normalized message, with numbers, GUIDs, IDs, and embedded JSON values stripped out.
- **Batching window:** how long is the window? Is it fixed, or does it start at the first error and close after a quiet period?
- **Content:** what goes in an error summary beyond the distinct errors and their counts? For example, a sample stack trace, or the log lines just before the error.
- **Per-channel limits:** do Slack and email need a rate limit or a cap on size during a long outage?

### Usage

- **Endpoint identification:** how does the daemon recognize that a line records an endpoint call, and pull out which endpoint it was? Does every app log requests the same way, or is this set up per app?
- **Endpoint inventory:** to report that an endpoint has *no* activity, the daemon needs a list of the endpoints that exist. Where does that list come from: configuration, app metadata such as Swagger, or endpoints that have been seen at least once?
- **Granularity:** what time bucket (hour or day)? Statistics are kept forever, so the bucket size sets how fast storage grows, and it can't be made finer later once the source logs are purged.

### Logging sufficiency

- **Minimum requirements:** what are they? For example: startup and shutdown are logged, every request is logged, errors include exceptions, there is a required set of fields, or an app logs at least once within some time span.
- **Reporting:** how often are sufficiency findings reported, and are they sent through the same channels as error summaries?

### Performance

- **What is measured:** which timings matter (request duration, job duration, timer lines that apps log), and how are they identified in each app's logs?
- **How app-specific rules are defined:** through configuration (patterns for each app), or through code extensions plugged into the daemon?

### Formats

- **Definition management:** how are definitions added, edited, and tested? Options are a command-line tool or messaging commands to the daemon. Either way, it should be able to test a definition against a sample file before saving it.
- **Detection threshold:** what share of lines must match before a format is accepted, and how many lines are sampled?
- **Initial library:** which formats are needed to cover the current apps?

### General

- **Deployment topology:** one daemon on each server reading local files, or one central daemon reading every server's log location over the network?
- **The active file versus "process once":** the newest file is still being written to. Is it read while it grows (fast error alerts, with its output replaced as more lines arrive), or only once it has rolled over (simpler, but errors are reported up to a day late)?
- **Backlog:** files are processed oldest first, so after downtime the daemon catches up through the backlog. Should errors found in old files during a catch-up send alerts, or only be stored? On first run, how far back should it go: all existing files (up to six months), or a configured cutoff?
- **Triggering reprocessing:** is a file reprocessed only when someone asks for it (for example through a messaging command), or also automatically when it changes after it was processed?
- **LLM provider:** which provider, and what data may be sent to it?

## Dependencies & Constraints

- Targets net10.0, because Albatross.Messaging only supports net10.0.
- Apps run on multiple servers. On each server, every app writes its logs to one central log location on that server, with one subfolder per app.
- Logs are rolling text files, so the watcher has to handle file rollover. File naming differs between apps.
- Log formats differ between apps. Some put the date, machine name, or thread ID in each line. At least one app writes a different format that has only the time of day in each line, so its date has to come from the file name.
- The log volume is too large for a person to read, so the daemon must work through it incrementally rather than rereading whole files.
- Log files are purged every six months. Daemon output is kept forever, so anything not captured from a file before it is purged is lost for good.
- Needs a SQL Server or PostgreSQL database to store its format definitions, state, and output. Every schema change must work on both.
