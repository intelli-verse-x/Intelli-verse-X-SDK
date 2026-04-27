import { Expectation } from "../assertions.js";

/**
 * Pushes a small set of `ivx_qa_*` metrics to a Prometheus pushgateway
 * so the SLO dashboard sees harness runs as first-class signals
 * (pass/fail + raw metric values). The Grafana dashboard JSON in
 * `infra/observability/multiplayer-slo.json` charts these directly.
 */
export async function pushProm(
  pushgateway: string,
  scriptName: string,
  metrics: Record<string, number>,
  result: {
    passed: number;
    failed: { exp: Expectation; reason: string }[];
  },
): Promise<void> {
  const lines: string[] = [];
  for (const [k, v] of Object.entries(metrics)) {
    lines.push(`# TYPE ivx_qa_${k} gauge`);
    lines.push(`ivx_qa_${k}{script="${scriptName}"} ${v}`);
  }
  lines.push(`# TYPE ivx_qa_assertion_pass_total counter`);
  lines.push(
    `ivx_qa_assertion_pass_total{script="${scriptName}"} ${result.passed}`,
  );
  lines.push(`# TYPE ivx_qa_assertion_fail_total counter`);
  lines.push(
    `ivx_qa_assertion_fail_total{script="${scriptName}"} ${result.failed.length}`,
  );
  const body = lines.join("\n") + "\n";
  const url = `${pushgateway.replace(/\/$/, "")}/metrics/job/ivx_qa_bot_harness/script/${encodeURIComponent(scriptName)}`;
  const res = await fetch(url, {
    method: "POST",
    headers: { "Content-Type": "text/plain" },
    body,
  });
  if (!res.ok) {
    throw new Error(`pushgateway POST failed: ${res.status} ${await res.text()}`);
  }
}
