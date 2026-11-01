# Specialized routing

Read only the section that matches the current task.

## Browser or UI debugging

Inspect the current URL, visible error, relevant element state, console/network evidence, and reproduction step. Avoid repeated full-page captures when the state has not changed.

## PHP, ThinkPHP, or deployment

Check framework/runtime version, entrypoint, routes, environment/database configuration, server configuration, and the relevant log window before broad source searches. For 404/install/admin/login issues, include document root and rewrite rules.

## Large documents and data

Inspect file size and structure first: workbook sheets and headings, document sections, PDF pages, or data schema. Read only the relevant range/section/error window and use a structured parser when available.

## Large repositories

Scan the directory tree once, exclude generated/vendor/dependency paths, perform a focused token audit/source map, then read in stages. Reuse summaries instead of reloading full source or generated output.
