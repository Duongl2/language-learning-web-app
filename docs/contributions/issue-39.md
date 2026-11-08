# Issue #39

Compute goals and current/longest streaks using the saved timezone, deduplicated client review identifiers and a grace period for the current day.

## Review scope

This branch contains only the changes for [issue #39](https://github.com/NguyenLuuKienn/language-learning-web-app/issues/39).

Prerequisites: None. Dependent draft PRs are intended to be reviewed after their prerequisites merge into develop. They do not copy prerequisite commits into their own diff.

## Verification

The complete split integration is validated separately with the full backend suite (including MySQL), frontend component tests, browser tests and production builds. Per-branch and prerequisite validation results are recorded in the PR body; do not assume that a dependent branch builds without its prerequisites.
