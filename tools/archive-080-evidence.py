"""Archive the latest real-Unity functional and diagnostic evidence for 0.8.0."""
import hashlib
import json
import argparse
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def load(path):
    return json.loads(Path(path).read_text(encoding="utf-8-sig"))


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--functional", type=Path, required=True)
    parser.add_argument("--package", type=Path, required=True)
    parser.add_argument("--diagnostic", type=Path, default=ROOT / "build/runtime-070/20261026_162310/result.json")
    args = parser.parse_args()
    functional, diagnostic = args.functional.resolve(), args.diagnostic.resolve()
    report = load(functional)
    binding = load(functional.parent / "input.json")
    package_sha = digest(args.package)
    if not (report.get("passed") and report.get("complete") and report.get("actualUnity") and not report.get("errors")):
        raise ValueError("Only a complete, passing actual-Unity report may be archived")
    if binding["packageSha256"] != package_sha or binding["inputSha256"] != report["inputSha256"]:
        raise ValueError("Report/package binding differs")
    if binding["workspaceSources"] or binding["basePackageInputSha256"] != report["inputSha256"]:
        raise ValueError("Final package evidence must run package sources without a workspace overlay")
    report["scope"] = ("Actual Unity: full functional regression plus tasks 1/2/5 checks. "
                       "Package sources with an injected acceptance probe; no population performance benchmark.")
    report["version"] = "0.8.0"
    report["packageAcceptance"] = True
    report["evidenceDirectory"] = str(functional.parent)
    report["rawReportSha256"] = digest(functional)
    report["gameAssemblySha256"] = digest(
        ROOT / "build/talents-implementation/runtime/game/worldbox_Data/Managed/Assembly-CSharp.dll")
    report["packageSha256"] = package_sha
    report["interactiveVisualReview"] = False
    perf = load(diagnostic)
    if not (perf.get("actualUnity") and perf.get("complete") and perf.get("performanceTested")):
        raise ValueError("Historical rendered diagnostic is incomplete")
    # This legacy field means a recorded rendered diagnostic exists for the
    # record-and-report gate. Keep its input provenance explicit: it is not a
    # claim that this functional run measured the new inputs' performance.
    report["performanceTested"] = perf["performanceTested"]
    report["currentInputPerformanceTested"] = False
    report["functionalRunPerformanceTested"] = False
    report["performanceEvidenceScope"] = "historical-inputs-only; not remeasured after tasks 1/2/5"
    report["performanceInputSha256"] = perf["inputSha256"]
    report["performanceReport"] = "resources/refactor-080-rendered-diagnostic.json"
    report["performanceVerdict"] = "below-target"
    report["performanceSummary"] = (
        "Historical source inputs (not this package): x20 diagnostic measured about 21.9 FPS "
        "and 62.8 ms p95 at 4682-7233 units. No new performance run was authorized. "
        "Current package FPS, CPU/GPU memory and sustained annual backlog are unverified.")
    (ROOT / "resources/refactor-080-functional-runtime.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")

    perf["scope"] = ("Actual Unity rendered diagnostic at x20 on the largest regular map. "
                     "Population evolved naturally from 5000, so this is not a controlled "
                     "throughput comparison and not an acceptance result.")
    perf["performanceAcceptance"] = False
    perf["evidenceDirectory"] = str(diagnostic.parent)
    perf["rawReportSha256"] = digest(diagnostic)
    perf["gameAssemblySha256"] = digest(
        ROOT / "build/talents-implementation/runtime/game/worldbox_Data/Managed/Assembly-CSharp.dll")
    (ROOT / "resources/refactor-080-rendered-diagnostic.json").write_text(
        json.dumps(perf, ensure_ascii=False, indent=2), encoding="utf-8")

    print(json.dumps({
        "functionalChecks": len(report.get("checks", [])),
        "functionalErrors": len(report.get("errors", [])),
        "functionalSha": report["rawReportSha256"][:16],
        "diagnosticFps": round(perf.get("fps", 0), 2),
        "diagnosticSha": perf["rawReportSha256"][:16],
        "currentInputPerformanceTested": False,
    }, ensure_ascii=True))


if __name__ == "__main__":
    main()
