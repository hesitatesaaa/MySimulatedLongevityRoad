(() => {
  if (!location.hash && "scrollRestoration" in history) {
    history.scrollRestoration = "manual";
    window.scrollTo(0, 0);
  }

  const revealTargets = document.querySelectorAll([
    ".number-ribbon > div",
    ".section-heading",
    ".world-layout",
    ".asset-tile",
    ".realm-track",
    ".path-note",
    ".career-art-wrap",
    ".career-tab",
    ".career-showcase",
    ".contract-line",
    ".bestiary-banner",
    ".codex-heading",
    ".beast-card",
    ".tide-panel",
    ".civilization-layout",
    ".building-feature",
    ".building-grid figure",
    ".map-atlas",
    ".map-marker-list figure",
    ".craft-card",
    ".inventory-codex",
    ".item-card",
    ".spell-codex",
    ".spell-card",
    ".return-art-wrap",
    ".return-layout",
    ".rebirth-steps article",
    ".history-stack article",
    ".download-layout"
  ].join(","));

  if (!("IntersectionObserver" in window)) return;
  document.body.classList.add("motion-supported");
  revealTargets.forEach((element, index) => {
    element.classList.add("reveal");
    const stagger = element.classList.contains("beast-card") ? (index % 4) * 55 : index % 3 * 65;
    element.style.setProperty("--reveal-delay", stagger + "ms");
  });

  const observer = new IntersectionObserver((entries, currentObserver) => {
    entries.forEach((entry) => {
      if (!entry.isIntersecting) return;
      entry.target.classList.add("in");
      currentObserver.unobserve(entry.target);
    });
  }, { threshold: 0.12, rootMargin: "0px 0px -28px 0px" });
  revealTargets.forEach((element) => observer.observe(element));

  const topbar = document.querySelector(".topbar");
  const updateNav = () => topbar.classList.toggle("is-scrolled", window.scrollY > 28);
  window.addEventListener("scroll", updateNav, { passive: true });
  updateNav();
})();
