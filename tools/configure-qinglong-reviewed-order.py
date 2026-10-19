"""One-off order correction after viewing all independently authored Qinglong frames."""
import importlib.util
from pathlib import Path

module_file = Path(__file__).with_name("actor-redraw.py")
spec = importlib.util.spec_from_file_location("authoring", module_file)
authoring = importlib.util.module_from_spec(spec)
spec.loader.exec_module(authoring)
path = authoring.RESOURCES / "actor-redraw.targets.json"
targets = authoring.load_json(path)
form = next(f for f in targets["forms"] if f["id"] == "mclsl_beast_qinglong")
form["actions"]["idle"]["frameOrder"] = [0, 7, 6, 1, 2, 3, 4, 8, 5, 9]
form["actions"]["attack"]["frameOrder"] = [0, 1, 2, 3, 4, 5, 6, 8, 7, 9, 10, 11, 12, 14, 13, 15]
authoring.save_json(path, targets)
print("Qinglong idle and attack order now follows reviewed poses; counts unchanged.")
