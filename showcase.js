(() => {
  const root = "./assets/game/";
  const careerInfo = {
    sword: {
      title: "剑修", seal: "剑", code: "sword", kicker: "SWORD CULTIVATOR · ACTIVE SKILLS",
      copy: "以剑入道，习得五阶主动与被动技能。战斗时依据目标、灵力与冷却条件自动出招。"
    },
    martial: {
      title: "武修", seal: "武", code: "martial", kicker: "MARTIAL CULTIVATOR · ACTIVE SKILLS",
      copy: "以武炼体，在近身交锋中磨砺自身。五阶技能随修行推进，战斗中自动施放。"
    },
    elemental: {
      title: "术修", seal: "术", code: "elemental", kicker: "ELEMENTAL CULTIVATOR · ACTIVE SKILLS",
      copy: "以灵力催动术法，掌握五阶主动与被动技能。出手受灵力、冷却与目标条件约束。"
    },
    sigil: {
      title: "符修", seal: "符", code: "sigil", kicker: "SIGIL CULTIVATOR · ACTIVE SKILLS",
      copy: "将修为寄于符箓，在攻守之间变换术式。五阶技能会按战斗条件自动触发。"
    },
    array: {
      title: "阵修", seal: "阵", code: "array", kicker: "FORMATION CULTIVATOR · ACTIVE SKILLS",
      copy: "以阵势应敌，逐阶修习主动与被动技能。具体施放仍受目标、灵力与冷却条件影响。"
    },
    spirit: {
      title: "灵契师", seal: "契", code: "spirit", kicker: "SPIRIT PACT CULTIVATOR · ACTIVE SKILLS",
      copy: "与契灵并肩修行，五阶职业技能会在战斗中自动施放；四种契灵外观随职业同行。"
    }
  };

  const tabs = Array.from(document.querySelectorAll(".career-tab"));
  const careerTitle = document.getElementById("career-name");
  const careerSeal = document.getElementById("career-seal");
  const careerKicker = document.getElementById("career-kicker");
  const careerCopy = document.getElementById("career-description");
  const skillRow = document.getElementById("career-skills");

  function selectCareer(tab) {
    const career = careerInfo[tab.dataset.career];
    if (!career) return;
    tabs.forEach((item) => {
      const selected = item === tab;
      item.classList.toggle("is-active", selected);
      item.setAttribute("aria-selected", String(selected));
      item.tabIndex = selected ? 0 : -1;
    });
    careerTitle.textContent = career.title;
    careerSeal.textContent = career.seal;
    careerKicker.textContent = career.kicker;
    careerCopy.textContent = career.copy;
    skillRow.setAttribute("aria-label", career.title + "五阶主动技能图标");
    skillRow.innerHTML = [1, 2, 3, 4, 5].map((rank) => {
      const rankName = ["壹阶", "贰阶", "叁阶", "肆阶", "伍阶"][rank - 1];
      return '<figure><img src="' + root + 'ui/CareerSkills/CP_' + career.code + "_" + rank + 'A.png" alt="' + career.title + rankName + '技能"><figcaption>' + rankName + "</figcaption></figure>";
    }).join("");
  }

  tabs.forEach((tab, index) => {
    tab.addEventListener("click", () => selectCareer(tab));
    tab.addEventListener("keydown", (event) => {
      if (!["ArrowRight", "ArrowLeft", "Home", "End"].includes(event.key)) return;
      event.preventDefault();
      const next = event.key === "Home" ? 0 : event.key === "End" ? tabs.length - 1 :
        (index + (event.key === "ArrowRight" ? 1 : tabs.length - 1)) % tabs.length;
      tabs[next].focus();
      selectCareer(tabs[next]);
    });
  });

  const beasts = [
    ["kunpeng", "鲲鹏", "扶摇垂天", "扇形风压，近处推力更强"],
    ["qinglong", "青龙", "苍龙御澜", "水击最多连锁三敌，伤害递减"],
    ["huanglong", "黄龙", "地脉龙震", "延迟地震，内圈重创、外圈减速"],
    ["zhuque", "朱雀", "离火焚邪", "火羽穿透并持续灼烧"],
    ["baihu", "白虎", "庚金虎煞", "爪击破甲，对已破甲者追加伤害"],
    ["qilin", "麒麟", "瑞麟护生", "治疗自身与附近最受伤的友方妖兽"],
    ["moqilin", "墨麒麟", "墨麟蚀光", "墨域降低敌人的伤害输出"],
    ["xuanwu", "玄武", "龟蛇玄甲", "护盾耗尽后触发近身反震"],
    ["fenghuang", "凤凰", "涅火回生", "低血量自愈，并净化灼烧与毒伤"],
    ["dafeng", "大风", "九霄风牢", "持续牵引区域内敌人并多次伤害"],
    ["manman", "蛮蛮", "比翼同心", "附近有同族时追加协同羽击"],
    ["xuanniao", "玄鸟", "玄音夺魄", "音波伤害并削减目标当前灵力"],
    ["qingluan", "青鸾", "青羽净障", "净化减速与束缚，短时加速抗控"],
    ["changliao", "长獠凶犬", "獠毒噬骨", "咬击施毒，再次命中引爆余毒"],
    ["mo", "貘", "吞梦入寐", "使目标入梦，受击即醒，成功后自身回灵"],
    ["hongmao", "红毛异兽", "赤血狂鬃", "消耗少量当前生命，短时强化普攻"],
    ["feiqiu", "飞球兽", "浮空震荡", "震退近敌并获得短时减伤"],
    ["yingying", "阴影兽", "逐影猎印", "累积后续攻击的影伤并延时结算"],
    ["goushou", "狗首妖兽", "犬祟封生", "咒印伤害并降低目标治疗收益"],
    ["liejiejing", "裂解鲸", "裂界鲸鸣", "区域震击后触发延迟裂爆"],
    ["tianshou", "兲兽", "镇天重压", "领域降低敌人移速与攻击速度"]
  ];
  const beastGrid = document.getElementById("beast-grid");
  beastGrid.innerHTML = beasts.map((beast, index) => {
    const [slug, name, power, effect] = beast;
    const number = String(index + 1).padStart(2, "0");
    return '<article class="beast-card" title="' + power + '：' + effect + '">' +
      '<span class="beast-number">NO. ' + number + '</span>' +
      '<div class="beast-specimen"><img src="' + root + 'ui/ActorIcons/mclsl_beast_' + slug + '.png" alt="' + name + '妖兽单位图标" loading="lazy"><span class="beast-power"><img src="' + root + 'ui/BeastTalents/BT_' + slug + '.png" alt="' + name + '本命神通图标" loading="lazy"></span></div>' +
      '<h4>' + name + '</h4><span class="beast-skill-name">' + power + '</span><p class="beast-effect">' + effect + '</p></article>';
  }).join("");

  const catalog = window.MCLSL_CATALOG || { items: [], spells: [] };
  const itemKinds = {
    Pill: { title: "丹药", mark: "丹", label: "PILLS" },
    Artifact: { title: "法宝", mark: "器", label: "ARTIFACTS" },
    Talisman: { title: "符箓", mark: "符", label: "TALISMANS" },
    Material: { title: "材料", mark: "材", label: "MATERIALS" }
  };
  const itemTiers = ["天阶", "地阶", "玄阶", "黄阶", "凡阶"];
  const inventoryPanel = document.getElementById("item-catalog-content");
  const spellPanel = document.getElementById("spell-catalog-content");

  function renderInventory(kind, tier) {
    const meta = itemKinds[kind];
    const selectedTier = tier || itemTiers[0];
    const items = catalog.items.filter((item) => item.kind === kind && item.tier === selectedTier);
    const tabId = "item-tier-tab-" + kind.toLowerCase();
    inventoryPanel.dataset.kind = kind;
    inventoryPanel.innerHTML = '<div class="item-category-heading"><span>' + meta.mark + '</span><div><span>QIANKUN / ' + meta.label + '</span><b>' + meta.title + '</b></div><small>' + catalog.items.filter((item) => item.kind === kind).length + ' 件珍藏</small></div>' +
      '<div class="tier-tabs" role="tablist" aria-label="' + meta.title + '品阶">' + itemTiers.map((rank, index) => '<button type="button" role="tab" id="' + tabId + '-' + index + '" aria-controls="item-tier-content" aria-selected="' + String(rank === selectedTier) + '" data-item-tier="' + rank + '" tabindex="' + (rank === selectedTier ? "0" : "-1") + '">' + rank + '</button>').join("") + '</div>' +
      '<div class="inventory-summary"><span>' + selectedTier + ' · ' + meta.title + '</span><b>' + items.length + ' 件</b><small>对应珍藏图标</small></div>' +
      '<div class="item-icon-grid" id="item-tier-content" role="tabpanel" aria-labelledby="' + tabId + '-' + itemTiers.indexOf(selectedTier) + '">' + items.map((item) => '<article class="item-card" title="' + selectedTier + ' ' + meta.title + '"><img src="' + root + 'ui/Items/' + item.id + '.png" alt="' + item.name + '图标" loading="lazy"><b>' + item.name + '</b></article>').join("") + '</div>';
    const tierTabs = Array.from(inventoryPanel.querySelectorAll(".tier-tabs [data-item-tier]"));
    tierTabs.forEach((tab, index) => {
      tab.addEventListener("click", () => renderInventory(kind, tab.dataset.itemTier));
      tab.addEventListener("keydown", (event) => {
        if (!["ArrowRight", "ArrowLeft", "Home", "End"].includes(event.key)) return;
        event.preventDefault();
        const next = event.key === "Home" ? 0 : event.key === "End" ? tierTabs.length - 1 :
          (index + (event.key === "ArrowRight" ? 1 : tierTabs.length - 1)) % tierTabs.length;
        const nextTier = tierTabs[next].dataset.itemTier;
        renderInventory(kind, nextTier);
        inventoryPanel.querySelector('[data-item-tier="' + nextTier + '"]').focus();
      });
    });
  }

  function renderSpells(realm) {
    const spells = catalog.spells.filter((spell) => spell.realm === realm);
    spellPanel.innerHTML = '<div class="spell-realm-heading"><span>' + realm + '境</span><b>' + spells.length + ' 道法术</b><small>达到相应境界后方可施放</small></div>' +
      '<div class="spell-card-grid">' + spells.map((spell) => '<article class="spell-card" title="' + spell.description + '"><img src="' + root + 'ui/Spells/' + spell.id + '.png" alt="' + spell.name + '图标" loading="lazy"><div><span>' + spell.law + '行</span><b>' + spell.name + '</b><small>' + spell.description + '</small></div></article>').join("") + '</div>';
  }

  function connectTabs(selector, attribute, render) {
    const tabList = Array.from(document.querySelectorAll(selector));
    function activate(tab) {
      tabList.forEach((candidate) => {
        const selected = candidate === tab;
        candidate.setAttribute("aria-selected", String(selected));
        candidate.tabIndex = selected ? 0 : -1;
      });
      render(tab.dataset[attribute]);
      const panelId = tab.getAttribute("aria-controls");
      const panel = panelId && document.getElementById(panelId);
      if (panel) panel.setAttribute("aria-labelledby", tab.id);
    }
    tabList.forEach((tab, index) => {
      tab.addEventListener("click", () => activate(tab));
      tab.addEventListener("keydown", (event) => {
        if (!["ArrowRight", "ArrowLeft", "Home", "End"].includes(event.key)) return;
        event.preventDefault();
        const next = event.key === "Home" ? 0 : event.key === "End" ? tabList.length - 1 :
          (index + (event.key === "ArrowRight" ? 1 : tabList.length - 1)) % tabList.length;
        tabList[next].focus();
        activate(tabList[next]);
      });
    });
    if (tabList.length) activate(tabList[0]);
  }

  connectTabs(".kind-tabs [data-kind]", "kind", renderInventory);
  connectTabs(".realm-tabs [data-realm]", "realm", renderSpells);

  const navToggle = document.getElementById("nav-toggle");
  const nav = document.getElementById("site-nav");
  navToggle.addEventListener("click", () => {
    const open = navToggle.getAttribute("aria-expanded") !== "true";
    navToggle.setAttribute("aria-expanded", String(open));
    navToggle.setAttribute("aria-label", open ? "关闭导航" : "打开导航");
    nav.classList.toggle("is-open", open);
  });
  nav.querySelectorAll("a").forEach((link) => link.addEventListener("click", () => {
    nav.classList.remove("is-open");
    navToggle.setAttribute("aria-expanded", "false");
    navToggle.setAttribute("aria-label", "打开导航");
  }));

  const numberRibbon = document.querySelector(".number-ribbon");
  if (numberRibbon && "IntersectionObserver" in window && !window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
    const counterObserver = new IntersectionObserver((entries) => {
      if (!entries[0].isIntersecting) return;
      counterObserver.disconnect();
      numberRibbon.querySelectorAll("[data-count]").forEach((counter) => {
        const target = Number(counter.dataset.count);
        const start = performance.now();
        const duration = 1500;
        const update = (now) => {
          const progress = Math.min((now - start) / duration, 1);
          const value = Math.round(target * (1 - Math.pow(1 - progress, 3)));
          counter.textContent = String(progress === 1 ? target : value).padStart(2, "0");
          if (progress < 1) requestAnimationFrame(update);
        };
        counter.textContent = "00";
        requestAnimationFrame(update);
      });
    }, { threshold: 0.25 });
    counterObserver.observe(numberRibbon);
  }
})();
