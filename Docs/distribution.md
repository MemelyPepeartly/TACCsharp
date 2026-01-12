# Distribution (Consumers)
This addon can be consumed via git submodule using the addon-only branch (`godot-addon`).

## How consumers should use it (submodule)
A consuming game repo should submodule the addon-only branch into tacc:

- `git submodule add -b godot-addon https://github.com/MemelyPepeartly/TACCsharp.git addons/tacc`

To update later in the consumer repo:
- `git submodule update --remote --merge addons/tacc`
- `git add addons/tacc && git commit -m "Bump TACC submodule"`
