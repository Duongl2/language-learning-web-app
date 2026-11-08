# Issue #31

Record enrollment and lesson states with monotonic completion, stable completion timestamps and optimistic concurrency. Return scoped course progress and the most recent unfinished lesson.

## Review scope

This branch contains only the changes for [issue #31](https://github.com/NguyenLuuKienn/language-learning-web-app/issues/31).

Prerequisites: None. Dependent draft PRs are intended to be reviewed after their prerequisites merge into develop. They do not copy prerequisite commits into their own diff.

## Verification

The complete split integration is validated separately with the full backend suite (including MySQL), frontend component tests, browser tests and production builds. Per-branch and prerequisite validation results are recorded in the PR body; do not assume that a dependent branch builds without its prerequisites.
