window.metrikaHit = function (url) {
    if (typeof ym !== "undefined") {
        ym(106196380, "hit", url);
    }
};
window.metrikaEvent = function (name) {
    ym(106196380, "reachGoal", name);
};