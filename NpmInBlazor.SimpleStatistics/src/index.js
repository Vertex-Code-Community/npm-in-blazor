import * as ss from "simple-statistics";

export function analyzeNumbers(values) {
    const nums = (values ?? []).map(Number).filter((x) => Number.isFinite(x));

    if (nums.length === 0) {
        return [{ metric: "error", value: "No valid numbers" }];
    }

    const sorted = [...nums].sort((a, b) => a - b);

    return [
        { metric: "count", value: nums.length },
        { metric: "min", value: ss.min(nums) },
        { metric: "max", value: ss.max(nums) },
        { metric: "mean", value: ss.mean(nums) },
        { metric: "median", value: ss.medianSorted(sorted) },
        { metric: "stdev(sample)", value: nums.length > 1 ? ss.sampleStandardDeviation(nums) : 0 },
        { metric: "variance(sample)", value: nums.length > 1 ? ss.sampleVariance(nums) : 0 },
        { metric: "q25", value: ss.quantileSorted(sorted, 0.25) },
        { metric: "q75", value: ss.quantileSorted(sorted, 0.75) },
    ];
}

export function analyzeColumn(rows, column) {
    const nums = (rows ?? [])
        .map((r) => r?.[column])
        .map(Number)
        .filter((x) => Number.isFinite(x));

    return analyzeNumbers(nums);
}

export function linearRegressionTable(rows, xKey, yKey) {
    const points = (rows ?? [])
        .map((r) => [Number(r?.[xKey]), Number(r?.[yKey])])
        .filter(([x, y]) => Number.isFinite(x) && Number.isFinite(y));

    if (points.length < 2) {
        return [{ metric: "error", value: "Need at least 2 valid (x,y) points" }];
    }

    const lr = ss.linearRegression(points);              // { m, b }
    const line = ss.linearRegressionLine(lr);            // function(x) => y

    const ys = points.map((p) => p[1]);
    const yMean = ss.mean(ys);
    const ssTot = ys.reduce((acc, y) => acc + (y - yMean) ** 2, 0);
    const ssRes = points.reduce((acc, [x, y]) => acc + (y - line(x)) ** 2, 0);
    const r2 = ssTot === 0 ? 1 : 1 - ssRes / ssTot;

    return [
        { metric: "count", value: points.length },
        { metric: "slope(m)", value: lr.m },
        { metric: "intercept(b)", value: lr.b },
        { metric: "r2", value: r2 },
    ];
}
