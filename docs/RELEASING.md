# Releasing TNID.NET

Author and publisher: **Codevia Solutions s.r.o.**

Version `0.1.0` was published through GitHub OIDC Trusted Publishing, confirmed
by the publisher on 2026-11-13. Release tag: `v0.1.0`, commit
`df32a573d81bf03cd9ffa78fd6e792a651c5b7bc`.
Package: [TNID.NET 0.1.0](https://www.nuget.org/packages/TNID.NET/0.1.0).
The local verification package hashes are historical local artifacts; they are
not claimed to be the hashes of the CI-built published package.

For subsequent releases:

1. Use the confirmed target repository
   `https://github.com/CodeviaSolutions/tnid.net.git` under `CodeviaSolutions`.
   Package repository and project URLs already match this repository. Inspect
   the remote's current branch/history before the separate publication step.
2. Retain the pinned TNID 1.0 revision `e7a1d5913ef012dfb9aa3a731e9dba02c7628982` and its
   MIT license review in `NOTICE.md`. Review future changes against that core
   format. The local official JSON is not approved for redistribution: establish
   its license before committing or packaging a copy, and include all required
   verbatim copyright and license notices wherever that material is distributed.
3. Retain the `TNID.NET` ID and the authorized Codevia Solutions NuGet owner.
   Choose a new, unpublished package version. The original HTTP 404 listing
   check predates the successful first publication.
4. Preserve the completed official-vector comparison and fixture hash recorded in
   `docs/VERIFICATION.md`; rerun it for implementation changes as described in
   `tests/EXTERNAL-VECTORS.md`. Encryption is an optional extension outside scope.
5. Preserve the completed SDK 11.0.100/net11.0 package-consumer evidence. For any
   candidate change, run `bash scripts/verify.sh --net11` and update the recorded
   results. To test an existing archive without repacking, use
   `bash scripts/verify-package.sh net11.0 /absolute/path/to/package.nupkg`.
   Inspect the package again after changes to bundled documentation.

GitHub publication is a separate step: create or use the public repository under
Codevia Solutions, push the reviewed source, let CI pass, then tag and publish a
GitHub release with the reviewed package attached. Do not include personal bios,
additional project authors, or tool credits. The MIT notice and provenance apply.

NuGet publication is another separate step: confirm package ownership, inspect the
exact package checksum and metadata, then publish through GitHub OIDC Trusted
Publishing using `.github/workflows/publish.yml`. Normal CI and branch pushes do
not publish. The release workflow runs only for version tags, verifies and packs
the library, then publishes the exact verified artifact in a separate job.
Do not assume publishing source to GitHub authorizes NuGet publication.

## Trusted Publishing setup

No long-lived API key is stored or entered. Configure NuGet's
[Trusted Publishing policy](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing):

- Policy/package owner: the authorized Codevia Solutions organization on NuGet.
- GitHub repository owner: `CodeviaSolutions`.
- Repository: `tnid.net`.
- Workflow file: `publish.yml` (the filename only).
- Environment: `nuget`.
- Package pattern: `TNID.NET`.
- Scope: publish new packages and package versions, including the first release.

In GitHub, create an environment named `nuget` and set the repository or environment
variable `NUGET_USER` to the NuGet profile username associated with the policy.
This is an individual NuGet login, not an email or the package author's company
name. The package author remains Codevia Solutions s.r.o. Any environment
protection rules must allow the release tag to deploy.

The first release was triggered by `v0.1.0`. For a subsequent release, commit and
push the updated version, README, and workflow, then create and push a new
matching version tag. Do not move or recreate an existing release tag.
The workflow checks that the tag matches the package
version, runs the official vectors and both packed consumers, checks the package
metadata and checksum, and obtains an ephemeral credential with `NuGet/login@v1`
immediately before pushing. A failure is not silently treated as a successful
duplicate publish. Confirm the workflow result and the NuGet listing afterward.

GitHub release creation remains a separate action. The workflow does not create
a GitHub release or change repository contents. For later package versions,
update the explicitly pinned package version in the project, consumers, scripts,
and release workflow before tagging.

Suggested release title: `TNID.NET 0.1.0`.
Suggested release body: “TNID.NET by Codevia Solutions s.r.o. implements the TNID 1.0
core format for .NET 10 with no third-party dependencies. Includes v0/v1 generation,
canonical text, explicit UUID conversion, and typed names. MIT licensed.” Attach
completed verification results, package checksum, and any known limitations.
