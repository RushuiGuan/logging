---
name: Albatross.LogAnalysis.Daemon
status: active
created: 2026-10-08T08:24:27-04:00
updated: 2026-10-27T15:00:00-04:00
---

## Business Requirements

Apps run on multiple servers and write rolling log files to a central log location on each server. These logs hold a lot of information, but nobody can read them all. The daemon gathers data from the logs for developers. What counts as useful changes over time, so each kind of analysis is a separate topic, and new topics can be added later. The first four topics, in priority order:

1. **What is going wrong?** *(first priority)* The daemon finds errors and reports a summary of them. An app can produce a burst of errors in a short time, so the summary has to work out *what* the errors are: their distinct causes, how many of each, and where they come from. A count such as "App X logged 500 errors" does not meet this requirement.
2. **What is being used?** *(second priority)* The daemon records usage statistics for each app and endpoint over time, so usage patterns can be queried later. This shows whether an endpoint is used at all. Lack of usage is a finding in itself: an endpoint with no log activity is either unused or not being logged. Usage is not sent as a report. It is stored for querying.
3. **Is logging sufficient?** *(third priority)* Apps must meet minimum logging requirements. The daemon checks each app's logs against those requirements and reports where they fall short, so the app can be fixed.
4. **How is it performing?** *(fourth priority)* The daemon pulls performance measurements out of the logs, such as how long operations take, so they can be benchmarked over time. What counts as a measurement is likely different for each app, so this is an app-specific capability rather than a general one.

Later topics are expected to include request logging and other metrics.

Developers are the primary readers. Reports can include technical detail such as exception types, source context, and representative messages.

## Technical Design

The daemon is a long-running service built on Albatross.Hosting. It can run as a Windows service, serves a web API, and uses Albatross.Messaging for its command bus.

The analysis engine has three parts:

- **Scanner:** finds app log folders under a log root and through configured entries, detects each file's format from the shared format library, groups lines into entries, and reads the opening of each entry: its timestamp and level. It knows nothing about any topic.
- **Topics:** each topic is a self-contained analysis written in C#. A topic declares which entries it wants (by level, app, source context, or text), extracts what it needs from them, and stores its own data. A topic may also deliver results, as the error topic does with alerts.
- **Runs:** a run applies a set of topics to a set of files. One pass over the files feeds every topic in the run. An entry gets a full parse only if at least one topic in the run wants it.

Topics that need fast results, such as errors, follow files as they grow. Other topics run as scans over files that have rolled over, on a schedule or on demand.

Format definitions, processing state, and all output are stored in one database, which can be SQL Server or PostgreSQL. Data access uses one shared Albatross.EFCore entity model, with a separate database session for each provider. Configuration picks the provider, and each provider has its own migrations.

## Key Design Decisions

### Analysis is organized as topics

Each kind of analysis is a topic. The scanner reads the files and the topics decide what is useful. The first four topics are errors, usage, logging sufficiency, and performance, and more can be added without changing the scanner. Topic logic is always written in C#, so a topic can do whatever the analysis requires. Defining whole topics as data was rejected, because data-defined rules can only express simple patterns. A topic can still read settings from the database, for example an app-specific pattern for its performance lines. Each topic owns the data it stores.

### Topics are built in priority order

Topics are built and tuned in priority order: errors first, usage second, logging sufficiency third, performance fourth. When they compete for effort or resources, the higher priority wins.

### A run feeds many topics in one pass

A run applies a set of topics to a set of files, and each file is read once for all of them. Scanning separately for each topic was rejected, because reading the files is the expensive part. Running a topic on its own over older files is still possible. That is how a new topic is backfilled.

### Live watching is decided by each topic

Topics that need fast results, such as errors, follow files as they grow. Other topics only read files that have rolled over. Watching every file live for every topic was rejected, because most topics gain nothing from it, and output for a file that is still growing has to be replaced again and again.

### Lines are parsed in two stages

The first stage reads only the opening of a line, the fields before the message. That is enough to tell whether the line starts a new entry, and to get its timestamp and level. The second stage, the full parse, runs only when a topic wants the entry. An entry no topic wants is skipped without a full parse, but it is still counted. An entry whose full parse fails keeps its timestamp, level, and raw text, so an unreadable error line is still reported. A line counts as a continuation of the previous entry only when its opening doesn't match. A line that starts an entry but fails the full parse is never treated as a continuation.

### Summaries target developers

Reports are written for developers. They keep technical detail: exception types, source context, and sample messages. Content is not simplified for non-technical readers.

### Files are processed in creation order

Each app's log files are processed in the order they were created, so statistics and findings build up in time order. Processing them out of order, or several at once for the same app, was rejected because each file's results depend on what came before it.

### Each file is processed once per topic

Processing state and output are tracked for each file and topic pair. A file is normally processed once for each topic. Processing it again for a topic replaces that topic's output for that file and leaves other topics alone, so a file can be rerun after a parser fix, a format change, or a change to a topic's logic without counting anything twice. Topics are versioned like format definitions, and output records the topic version that produced it. A file can only be processed while it still exists, which means within the six months before log files are purged. That window also limits how far back a new topic can be backfilled.

### Daemon output is permanent data

Everything the topics store is kept for good, even after the log files it came from have been purged. A missing source file never causes its output to be deleted. This output is the only long-term record of what the logs said, so the daemon's output is treated as data and the log files are treated as temporary input.

### Error summaries identify distinct errors

A summary breaks errors down into distinct causes, each with a count, a time range, its source, and a representative message. A plain count of errors for each app was rejected, because during a burst the developer needs to know what broke, not just how much logging happened.

### Every error is reported

Every Error or Fatal entry is included in a summary. There is no "already seen" filter. Holding back errors that look like ones seen before was rejected, because a recurring error is still an error a developer needs to know about.

### Errors are batched briefly before sending

Errors are collected over a short window, and each window goes out as one summary. Sending one alert per error was rejected, because a single failure, such as a dependency going down, can produce a flood of errors.

### Only Error and Fatal levels are reported

The error topic ignores Warning and lower levels. Including warnings was rejected because they are noisy, and that would hide the real errors. Other topics choose their own levels.

### Usage is stored, not reported

Usage statistics go to the database for later querying, and no usage report is sent. The value of usage data is in trends over time and in finding endpoints with no activity, and both are better served by queries than by a pushed report.

### Performance benchmarks are app-specific

The performance topic is set up for each app individually, through app-specific settings that its C# logic reads. One general rule for every app was rejected, because apps don't log timings in a shared format. An app with no performance setup is skipped by this topic. Like usage, benchmark statistics are stored to be queried, not sent as reports.

### Formats come from a shared definition library

Logs are plain text in a small number of distinct formats, so each format is defined once, given a name, and shared by every app that uses it. Defining a format for each app was rejected, because many apps share the same template. A Serilog format is defined by its output template, the same string developers write in their Serilog configuration. A format from another logging library is defined by a regular expression with named fields, and can say how to get the date from the file name when lines only have the time of day. Level names are mapped to one standard set. Embedded JSON is treated as part of the message.

### Each file's format is detected

The daemon finds a file's format by trying the known definitions against the first lines of the file and picking the best match. Configuration can pin an app to a format when detection gets it wrong. A required mapping from each app to its format was rejected, because it is one more list to keep up to date. Detecting per file also handles an app that changes its template partway through its life. A file that matches no format well enough is reported as a logging sufficiency finding.

### Format definitions are versioned

Each change to a format definition creates a new version, and output records which version produced it. That shows which files were parsed with a faulty definition and need reprocessing while their source files still exist.

### Log files are found by discovery plus configuration

The daemon discovers app logs under a log root (one subfolder per app). Configuration can add locations outside that root, exclude discovered ones, and pin an app to a format. This way new apps are picked up with little setup, and odd layouts can still be handled.

### Rules first, LLM optional

A deterministic stage always produces the error summary by grouping and counting entries. An LLM can optionally add a narrative on top of that output, but it is never the only path. Summaries still work, and stay the same every run, when no LLM is configured. Sending raw logs straight to an LLM was rejected because of cost, privacy, and inconsistent output.

### Multiple delivery channels

Topics that report, such as errors and logging sufficiency, can deliver to files, Slack, and email. Each channel is optional and is configured on its own.

### Everything is kept in one database

Format definitions, configuration, processing state, and all output live in one database. Keeping everything in files was rejected for three reasons. Daemons on several servers need to share the same definitions. The permanent output has to be queryable across all apps. And with one store, only one thing has to be deployed and backed up. The cost is that definitions are no longer files tracked in source control, so they need their own management tool.

### Both databases are first-class

SQL Server and PostgreSQL are equally supported. Neither one is the primary target, and the provider is chosen in configuration. There is one entity model built on Albatross.EFCore, a database session for each provider, and migrations generated separately for each. Any schema, query, or migration change is incomplete until it works on both providers. Supporting only one engine was rejected, because the daemon should run on whichever database a team already operates.

## Open Questions

### Runs and topics

- **Starting runs:** which runs are scheduled, and which are started on demand, for example from the admin tool or a messaging command? Who chooses the topics and files for an on-demand run?
- **Topic settings:** what shape do settings take for each topic, such as per-app patterns for performance, and how are they managed?

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

### Formats

- **Definition management:** how are definitions added, edited, and tested? Options are a command-line tool or messaging commands to the daemon. Either way, it should be able to test a definition against a sample file before saving it.
- **Detection threshold:** what share of lines must match before a format is accepted, and how many lines are sampled?
- **Initial library:** which formats are needed to cover the current apps?

### General

- **Deployment topology:** one daemon on each server reading local files, or one central daemon reading every server's log location over the network?
- **Backlog:** files are processed oldest first, so after downtime the daemon catches up through the backlog. Should errors found in old files during a catch-up send alerts, or only be stored? On first run, how far back should it go: all existing files (up to six months), or a configured cutoff?
- **Triggering reprocessing:** is a file reprocessed only when someone asks for it, or also automatically when it changes after it was processed, or when a topic or format version changes?
- **LLM provider:** which provider, and what data may be sent to it?

## Dependencies & Constraints

- Targets net10.0, because Albatross.Messaging only supports net10.0.
- Apps run on multiple servers. On each server, every app writes its logs to one central log location on that server, with one subfolder per app.
- Logs are rolling text files, so the watcher has to handle file rollover. File naming differs between apps.
- Log formats differ between apps. Some put the date, machine name, or thread ID in each line. At least one app writes a different format that has only the time of day in each line, so its date has to come from the file name.
- The log volume is too large for a person to read, so the daemon must work through it incrementally rather than rereading whole files.
- Log files are purged every six months. Daemon output is kept forever, so anything not captured from a file before it is purged is lost for good. This also limits how far back a new topic can be backfilled.
- Needs a SQL Server or PostgreSQL database to store its format definitions, state, and output. Every schema change must work on both.
