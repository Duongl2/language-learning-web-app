# Issue #33

Provide CSV template download, strict UTF-8 browser decoding, a bounded quoted-field parser and physical-line validation preview. This PR does not import cards.

## Review scope

This branch contains only the changes for [issue #33](https://github.com/NguyenLuuKienn/language-learning-web-app/issues/33).

Prerequisites: #30. Dependent draft PRs are intended to be reviewed after their prerequisites merge into develop. They do not copy prerequisite commits into their own diff.

## Verification

The complete split integration is validated separately with the full backend suite (including MySQL), frontend component tests, browser tests and production builds. Per-branch and prerequisite validation results are recorded in the PR body; do not assume that a dependent branch builds without its prerequisites.
