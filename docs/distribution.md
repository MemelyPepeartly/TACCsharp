## Using TACCsharp as a Godot Addon (Distribution Branch Workflow)

This repo is both:
- a full Godot project (demos, docs, schemas, etc.) on `main`, and
- a reusable Godot addon under tacc.

To make the addon easy to consume as a git submodule (so users can mount it directly at `res://addons/tacc`), we publish an **addon-only branch** named `godot-addon`. That branch contains *only* the contents of tacc at the repository root.

### Why we do this
Git submodules can only point at an entire repo, not a subfolder. If a consumer submodules `main`, they’d get `addons/tacc/addons/tacc/...` which Godot won’t treat as a normal addon path. The `godot-addon` branch fixes this by making the addon folder the repo root for that branch.

---

## Update process (For any wonderful forkers out there)

### 1) Do all development on `main`
1. Checkout `main` and pull the latest:
   - `git checkout main`
   - `git pull`
2. Make changes under tacc (and anywhere else in the repo as needed).
3. Commit normally on `main` and push as usual:
   - `git add -A`
   - `git commit -m "..."`  
   - `git push`

This keeps `main` as the source of truth.

---

### 2) Re-publish the addon-only branch (`godot-addon`)
From the repo root:

1. Create/update the `godot-addon` branch by splitting out tacc:
   - `git subtree split --prefix=addons/tacc -b godot-addon`

2. Push the regenerated branch to origin:
   - `git push origin godot-addon:godot-addon --force`

Notes:
- The `--force` is expected here because `git subtree split` generates a new commit history for the extracted branch. Treat `godot-addon` as a **build artifact/distribution branch**, not a place to do direct development.

---

## How consumers should use it (submodule)
A consuming game repo should submodule the addon-only branch into tacc:

- `git submodule add -b godot-addon https://github.com/MemelyPepeartly/TACCsharp.git addons/tacc`

To update later in the consumer repo:
- `git submodule update --remote --merge addons/tacc`
- `git add addons/tacc && git commit -m "Bump TACC submodule"`

---

## Guardrails / best practices
- Avoid committing directly to `godot-addon`. Always start from `main` and regenerate.
- Consider tagging releases on `main` (e.g. `v0.7.0`), then regenerating `godot-addon` from that release commit for reproducibility.
- If you change the addon folder name or structure, update `--prefix=addons/tacc` accordingly.

--- 