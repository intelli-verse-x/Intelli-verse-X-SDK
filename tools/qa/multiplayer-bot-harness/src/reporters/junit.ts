import * as fs from "node:fs";
import { Expectation } from "../assertions.js";

export function writeJUnit(
  outPath: string,
  scriptName: string,
  result: {
    passed: number;
    failed: { exp: Expectation; reason: string }[];
  },
  metrics: Record<string, number>,
): void {
  const total = result.passed + result.failed.length;
  const cases = [
    ...new Array(result.passed)
      .fill(0)
      .map((_, i) => `    <testcase classname="${scriptName}" name="passed-${i}"/>`)
      .join("\n"),
    ...result.failed.map(
      (f) =>
        `    <testcase classname="${scriptName}" name="${escapeXml(f.exp.expression)}">\n` +
        `      <failure message="${escapeXml(f.reason)}"/>\n` +
        `    </testcase>`,
    ),
  ].join("\n");
  const props = Object.entries(metrics)
    .map(([k, v]) => `      <property name="${k}" value="${v}"/>`)
    .join("\n");
  const xml =
    `<?xml version="1.0" encoding="UTF-8"?>\n` +
    `<testsuite name="ivx-bot-harness/${scriptName}" tests="${total}" ` +
    `failures="${result.failed.length}">\n` +
    `  <properties>\n${props}\n  </properties>\n` +
    `${cases}\n` +
    `</testsuite>\n`;
  fs.writeFileSync(outPath, xml, "utf8");
}

function escapeXml(s: string): string {
  return s
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}
