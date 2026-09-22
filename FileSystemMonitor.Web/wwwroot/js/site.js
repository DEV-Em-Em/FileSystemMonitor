document.addEventListener("DOMContentLoaded", () => {
    const form = document.getElementById("analysisForm");
    const progress = document.getElementById("progress");
    const button = document.getElementById("analyzeButton");

    form?.addEventListener("submit", () => {
        progress?.classList.remove("hidden");
        if (button) {
            button.disabled = true;
            button.textContent = "Analyzing...";
        }
    });

    document.querySelectorAll(".tree-toggle").forEach(toggle => {
        toggle.addEventListener("click", () => {
            const node = toggle.closest(".tree-node");
            const children = node?.nextElementSibling;
            if (!children) return;

            const collapsed = children.style.display === "none";
            children.style.display = collapsed ? "" : "none";
            toggle.textContent = collapsed ? "▾" : "▸";
        });
    });
});
