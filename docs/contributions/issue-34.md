# Issue #34

Reuse the preview parser and require explicit confirmation before atomically appending valid rows. Add owner-only export with quoting, UTF-8 BOM and formula injection defenses.

## Review scope

This branch contains only the changes for [issue #34](https://github.com/NguyenLuuKienn/language-learning-web-app/issues/34).

Prerequisites: #33. Dependent draft PRs are intended to be reviewed after their prerequisites merge into develop. They do not copy prerequisite commits into their own diff.

## Verification

The complete split integration is validated separately with the full backend suite (including MySQL), frontend component tests, browser tests and production builds. Per-branch and prerequisite validation results are recorded in the PR body; do not assume that a dependent branch builds without its prerequisites.
