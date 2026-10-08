"use strict";

const portfolioRoot = document.querySelector("#portfolio");
const projectRoot = document.querySelector("#project-detail");
const statusMessage = document.querySelector("#status");
const detailStatus = document.querySelector("#detail-status");
const searchPage = document.querySelector("#search-page");
const searchForm = document.querySelector("#search-form");
const searchInput = document.querySelector("#search-query");
const searchResults = document.querySelector("#search-results");
const searchFilters = Array.from(document.querySelectorAll('input[name="type"]'));

function escapeHtml(value) {
  return String(value ?? "").replace(/[&<>"']/g, character => ({
    "&": "&amp;",
    "<": "&lt;",
    ">": "&gt;",
    '"': "&quot;",
    "'": "&#39;"
  })[character]);
}

function safeUrl(value) {
  if (!value) return null;
  try {
    const url = new URL(value, window.location.origin);
    return ["http:", "https:", "mailto:"].includes(url.protocol) ? url.href : null;
  } catch {
    return null;
  }
}

function formatDate(date) {
  if (!date) return "Present";
  const parsed = new Date(`${date}T00:00:00`);
  return Number.isNaN(parsed.valueOf())
    ? date
    : new Intl.DateTimeFormat(undefined, { month: "short", year: "numeric" }).format(parsed);
}

function section(index, title, id, content) {
  return `<section id="${id}" class="section reveal">
    <div class="section-heading"><span class="section-index">${index}</span><h2>${title}</h2></div>
    <div class="section-content">${content}</div>
  </section>`;
}

function resultCard(result) {
  const externalUrl = safeUrl(result.externalUrl);
  const avatarUrl = result.type === "profile" && safeUrl(result.avatarUrl)
    ? `<img class="result-avatar" src="${escapeHtml(safeUrl(result.avatarUrl))}" alt="" loading="lazy">`
    : `<span class="result-avatar-placeholder" aria-hidden="true">${escapeHtml(result.type.slice(0, 1).toUpperCase())}</span>`;
  return `<article class="result-card">
    <div class="result-card-top">${avatarUrl}<div>
      <div class="result-meta"><span class="result-type">${escapeHtml(result.type)}</span>${result.context ? `<span>${escapeHtml(result.context)}</span>` : ""}</div>
      <h2><a href="${escapeHtml(result.href)}">${escapeHtml(result.title)} <span aria-hidden="true">↗</span></a></h2>
    </div></div>
    <p>${escapeHtml(result.description)}</p>
    ${externalUrl ? `<a class="result-external" href="${escapeHtml(externalUrl)}" target="_blank" rel="noopener noreferrer">Open external link ↗</a>` : ""}
  </article>`;
}

function renderSearchResults(payload) {
  const counts = new Map();
  for (const result of payload.results) counts.set(result.type, (counts.get(result.type) || 0) + 1);
  const summary = Array.from(counts, ([type, count]) =>
    `<span><strong>${count}</strong> ${escapeHtml(type)}${count === 1 ? "" : "s"}</span>`).join("");
  searchResults.innerHTML = `
    <div class="results-heading">
      <div><p class="section-index">DIRECTORY / ${String(payload.total).padStart(2, "0")} MATCHES</p><h2>${payload.query ? `Results for “${escapeHtml(payload.query)}”` : "Browse the directory"}</h2></div>
      <p class="result-counts">${summary || "Try another search or filter."}</p>
    </div>
    ${payload.results.length
      ? `<div class="results-grid">${payload.results.map(resultCard).join("")}</div>`
      : `<p class="no-results">No matches yet. Try a broader query or enable another result type.</p>`}`;
  searchResults.hidden = false;
}

function selectedSearchTypes() {
  return searchFilters.filter(filter => filter.checked).map(filter => filter.value);
}

async function performSearch({ updateHistory = true } = {}) {
  const selectedTypes = selectedSearchTypes();
  if (selectedTypes.length === 0) {
    statusMessage.textContent = "Select at least one result type.";
    statusMessage.hidden = false;
    searchResults.hidden = true;
    return;
  }

  const query = searchInput.value.trim();
  const pageQuery = new URLSearchParams();
  if (query) pageQuery.set("q", query);
  for (const type of selectedTypes) pageQuery.append("type", type);
  if (updateHistory) {
    history.pushState(null, "", `/search?${pageQuery.toString()}`);
  }

  const apiQuery = new URLSearchParams({ types: selectedTypes.join(",") });
  if (query) apiQuery.set("q", query);
  statusMessage.hidden = false;
  statusMessage.textContent = "Searching profiles, projects, skills, and links…";
  searchResults.hidden = true;
  try {
    const response = await fetch(`/api/search?${apiQuery.toString()}`, {
      headers: { Accept: "application/json" }
    });
    if (!response.ok) throw new Error(`Request failed (${response.status}).`);
    renderSearchResults(await response.json());
    statusMessage.hidden = true;
    document.title = query ? `Search: ${query} · Portfolio directory` : "Browse · Portfolio directory";
  } catch (error) {
    statusMessage.textContent = `Could not search the directory. ${error.message}`;
  }
}

function loadSearchFromUrl() {
  const params = new URLSearchParams(window.location.search);
  searchInput.value = params.get("q") || "";
  const requestedTypes = [
    ...params.getAll("type").flatMap(value => value.split(",")),
    ...params.getAll("types").flatMap(value => value.split(","))
  ].filter(Boolean);
  for (const filter of searchFilters) {
    filter.checked = requestedTypes.length === 0 || requestedTypes.includes(filter.value);
  }
  void performSearch({ updateHistory: false });
}

searchForm.addEventListener("submit", event => {
  event.preventDefault();
  void performSearch();
});
searchFilters.forEach(filter => filter.addEventListener("change", () => void performSearch()));

function renderSkills(skills) {
  if (!skills?.length) return `<p class="empty-note">No skills listed yet.</p>`;
  const groups = new Map();
  for (const skill of skills) {
    const category = skill.category || "Other";
    if (!groups.has(category)) groups.set(category, []);
    groups.get(category).push(skill);
  }
  return `<div class="skills-grid">${Array.from(groups, ([category, items]) => `
    <div class="skill-group">
      <h3>${escapeHtml(category)}</h3>
      <ul class="skill-list">${items.map(skill => `<li><a href="/search?q=${encodeURIComponent(skill.name)}">${escapeHtml(skill.name)}</a></li>`).join("")}</ul>
    </div>`).join("")}</div>`;
}

function renderProjects(projects) {
  if (!projects?.length) return `<p class="empty-note">No projects listed yet.</p>`;
  return `<ol class="project-list">${projects.map((project, index) => {
    const links = [
      project.demoUrl && safeUrl(project.demoUrl)
        ? `<a href="${escapeHtml(safeUrl(project.demoUrl))}" target="_blank" rel="noopener noreferrer">Live demo ↗</a>`
        : "",
      project.sourceUrl && safeUrl(project.sourceUrl)
        ? `<a href="${escapeHtml(safeUrl(project.sourceUrl))}" target="_blank" rel="noopener noreferrer">Source ↗</a>`
        : ""
    ].filter(Boolean).join("");
    const skills = project.skills?.length
      ? `<ul class="tag-list">${project.skills.map(skill => `<li>${escapeHtml(skill.name)}</li>`).join("")}</ul>`
      : "";
    return `<li class="project-row">
      <span class="project-number">${String(index + 1).padStart(2, "0")}</span>
      <div>
        <p class="eyebrow">${escapeHtml(project.organization || "Independent project")}${project.role ? ` <span>/</span> ${escapeHtml(project.role)}` : ""}</p>
        <h3><a class="project-title-link" href="/project/${encodeURIComponent(project.slug)}">${escapeHtml(project.title)} <span aria-hidden="true">↗</span></a></h3>
        <p class="project-summary">${escapeHtml(project.summary)}</p>
        ${project.description ? `<p class="project-description">${escapeHtml(project.description)}</p>` : ""}
        <p class="project-dates">${formatDate(project.startDate)} — ${formatDate(project.endDate)}</p>
        ${skills}
        ${links ? `<div class="project-links">${links}</div>` : ""}
      </div>
    </li>`;
  }).join("")}</ol>`;
}

function renderExperience(experiences) {
  if (!experiences?.length) return `<p class="empty-note">No experience listed yet.</p>`;
  return `<ol class="timeline">${experiences.map(role => `
    <li class="timeline-item">
      <p class="eyebrow">${formatDate(role.startDate)} — ${role.isCurrent ? "Present" : formatDate(role.endDate)}</p>
      <h3>${escapeHtml(role.jobTitle)}</h3>
      <p class="timeline-company">${escapeHtml(role.company)}</p>
      ${role.description ? `<p class="timeline-description">${escapeHtml(role.description)}</p>` : ""}
    </li>`).join("")}</ol>`;
}

function renderSocialLinks(links, email) {
  const safeLinks = (links || []).map(link => {
    const url = safeUrl(link.url);
    return url ? `<a class="social-link" href="${escapeHtml(url)}" target="_blank" rel="noopener noreferrer">${escapeHtml(link.label)} ↗</a>` : "";
  }).filter(Boolean);
  const safeEmail = safeUrl(email ? `mailto:${email}` : null);
  if (safeEmail) safeLinks.unshift(`<a class="social-link" href="${escapeHtml(safeEmail)}">Email ↗</a>`);
  return safeLinks.length
    ? `<div class="contact-links">${safeLinks.join("")}</div>`
    : `<p class="empty-note">No contact links listed.</p>`;
}

function renderProfile(profile) {
  const location = profile.location
    ? `<p class="location"><span aria-hidden="true">⌖</span> ${escapeHtml(profile.location)}</p>`
    : "";
  const avatar = `<img class="avatar" src="/api/portfolios/${profile.id}/avatar" alt="Pixel avatar for ${escapeHtml(profile.displayName)}">`;

  portfolioRoot.innerHTML = `
    <section id="top" class="hero">
      <div class="hero-grid" aria-hidden="true"></div>
      <div class="hero-copy">
        <p class="command"><span>$</span> whoami</p>
        <h1>${escapeHtml(profile.displayName)}<span>.</span></h1>
        <p class="headline">${escapeHtml(profile.headline)}</p>
        ${profile.bio ? `<p class="bio">${escapeHtml(profile.bio)}</p>` : ""}
        ${location}
        <div class="hero-actions">
          <a class="primary-link" href="#projects">Explore projects <span aria-hidden="true">↓</span></a>
          ${profile.resumeUrl && safeUrl(profile.resumeUrl) ? `<a class="text-link" href="${escapeHtml(safeUrl(profile.resumeUrl))}" target="_blank" rel="noopener noreferrer">Résumé ↗</a>` : ""}
        </div>
      </div>
      <div class="hero-card">
        <div class="avatar-wrap">${avatar}</div>
        <p class="card-label">PROFILE / ${String(profile.id).padStart(3, "0")}</p>
        <p class="card-name">${escapeHtml(profile.displayName)}</p>
        <p class="card-role">${escapeHtml(profile.headline)}</p>
        ${location}
      </div>
      <div class="hero-proof">
        <div><strong>${String(profile.projects?.length || 0).padStart(2, "0")}</strong><span>Projects</span></div>
        <div><strong>${String(profile.experiences?.length || 0).padStart(2, "0")}</strong><span>Experience</span></div>
        <div><strong>${String(profile.skills?.length || 0).padStart(2, "0")}</strong><span>Technologies</span></div>
      </div>
    </section>
    ${section("01", "Selected projects", "projects", renderProjects(profile.projects))}
    ${section("02", "Experience", "experience", renderExperience(profile.experiences))}
    ${section("03", "Skills & technologies", "skills", renderSkills(profile.skills))}
    ${section("04", "Get in touch", "contact", renderSocialLinks(profile.socialLinks, profile.contactEmail))}
    <div class="profile-footer"><span class="mono">END OF PROFILE ${String(profile.id).padStart(3, "0")}</span><a href="#top">Back to top ↑</a></div>`;

  portfolioRoot.hidden = false;
}

async function loadProfile(profileKey) {
  searchPage.hidden = true;
  detailStatus.hidden = false;
  portfolioRoot.hidden = true;
  detailStatus.textContent = "Loading profile…";

  try {
    const response = await fetch(`/api/portfolios/${encodeURIComponent(profileKey)}`, {
      headers: { Accept: "application/json" }
    });
    if (response.status === 404) {
      detailStatus.textContent = `No profile was found for “${profileKey}”. Browse other profiles in the <a href="/search?type=profiles">directory</a>.`;
      return;
    }
    if (!response.ok) throw new Error(`Request failed (${response.status}).`);
    renderProfile(await response.json());
    detailStatus.hidden = true;
    document.title = `${profileKey} · Portfolio`;
  } catch (error) {
    detailStatus.textContent = `Could not load this profile. ${error.message}`;
  }
}

function renderProject(project) {
  const skills = project.skills?.length
    ? `<ul class="tag-list">${project.skills.map(skill => `<li><a href="/search?q=${encodeURIComponent(skill.name)}">${escapeHtml(skill.name)}</a></li>`).join("")}</ul>`
    : `<p class="empty-note">No technologies listed yet.</p>`;
  const profiles = project.profiles?.length
    ? `<ul class="related-profiles">${project.profiles.map(profile => `<li><a href="/profile/${profile.id}"><img class="related-avatar" src="${escapeHtml(profile.avatarUrl)}" alt="">${escapeHtml(profile.displayName)}${profile.role ? ` <span>/ ${escapeHtml(profile.role)}</span>` : ""} ↗</a></li>`).join("")}</ul>`
    : `<p class="empty-note">No profiles linked to this project yet.</p>`;
  const externalLinks = [
    project.demoUrl && safeUrl(project.demoUrl) ? `<a class="primary-link" href="${escapeHtml(safeUrl(project.demoUrl))}" target="_blank" rel="noopener noreferrer">Open live demo ↗</a>` : "",
    project.sourceUrl && safeUrl(project.sourceUrl) ? `<a class="text-link" href="${escapeHtml(safeUrl(project.sourceUrl))}" target="_blank" rel="noopener noreferrer">View source ↗</a>` : ""
  ].filter(Boolean).join("");

  projectRoot.innerHTML = `
    <a class="back-link" href="/search?type=projects">← Back to project results</a>
    <section class="project-hero">
      <p class="command"><span>$</span> project --slug ${escapeHtml(project.slug)}</p>
      <h1>${escapeHtml(project.title)}<span>.</span></h1>
      <p class="project-summary">${escapeHtml(project.summary)}</p>
      ${externalLinks ? `<div class="hero-actions">${externalLinks}</div>` : ""}
    </section>
    ${section("01", "About this project", "about", project.description
      ? `<p class="project-description project-long-description">${escapeHtml(project.description)}</p>`
      : `<p class="empty-note">No additional description available.</p>`)}
    ${section("02", "Technologies", "skills", skills)}
    ${section("03", "People", "profiles", profiles)}
    <div class="profile-footer"><span class="mono">PROJECT / ${String(project.id).padStart(3, "0")}</span><a href="/search">Explore the directory ↗</a></div>`;
  projectRoot.hidden = false;
}

async function loadProject(slug) {
  searchPage.hidden = true;
  detailStatus.hidden = false;
  projectRoot.hidden = true;
  detailStatus.textContent = "Loading project…";
  try {
    const response = await fetch(`/api/projects/${encodeURIComponent(slug)}`, {
      headers: { Accept: "application/json" }
    });
    if (response.status === 404) {
      detailStatus.textContent = `No project was found for “${slug}”. Browse more in the directory.`;
      return;
    }
    if (!response.ok) throw new Error(`Request failed (${response.status}).`);
    const project = await response.json();
    renderProject(project);
    detailStatus.hidden = true;
    document.title = `${project.title} · Portfolio project`;
  } catch (error) {
    detailStatus.textContent = `Could not load this project. ${error.message}`;
  }
}

const insightsPage = document.querySelector("#insights-page");
const insightsForm = document.querySelector("#insights-form");
const insightsStatus = document.querySelector("#insights-status");
const insightsResults = document.querySelector("#insights-results");
const insightsFilterNames = ["category", "location", "employmentType", "workMode"];
const insightsPageSize = 20;

function fillSelect(select, values, selected) {
  select.innerHTML = `<option value="">All</option>` + values
    .map(value => `<option value="${escapeHtml(value)}"${value.toLowerCase() === (selected || "").toLowerCase() ? " selected" : ""}>${escapeHtml(value)}</option>`)
    .join("");
}

function insightsQueryFromForm(page) {
  const params = new URLSearchParams();
  const q = insightsForm.elements.q.value.trim();
  if (q) params.set("q", q);
  for (const name of insightsFilterNames) {
    const value = insightsForm.elements[name].value;
    if (value) params.set(name, value);
  }
  if (page > 1) params.set("page", String(page));
  return params;
}

function segmentRow(segment) {
  const skillHref = `/search?q=${encodeURIComponent(segment.skill)}&type=skills`;
  const locationHref = `/insights?location=${encodeURIComponent(segment.location)}`;
  const companyHref = `/insights?q=${encodeURIComponent(segment.company)}`;
  const candidates = segment.sampleCandidates.map(candidate => `<a href="/profile/${encodeURIComponent(candidate.profileId)}">
      <img src="${escapeHtml(safeUrl(candidate.avatarUrl) || "")}" alt="" loading="lazy">${escapeHtml(candidate.displayName)}</a>`).join("");
  return `<tr>
    <td><span class="pill">${escapeHtml(segment.category)}</span><br><a href="${escapeHtml(skillHref)}">${escapeHtml(segment.skill)}</a></td>
    <td><a href="${escapeHtml(locationHref)}">${escapeHtml(segment.location || "—")}</a></td>
    <td><a href="${escapeHtml(companyHref)}">${escapeHtml(segment.company)}</a><br><span class="muted">${escapeHtml(segment.jobTitle)}</span></td>
    <td><span class="pill">${escapeHtml(segment.employmentType)}</span> <span class="pill">${escapeHtml(segment.workMode)}</span></td>
    <td class="count">${segment.candidateCount}</td>
    <td><div class="candidate-stack">${candidates}</div></td>
  </tr>`;
}

function renderInsights(payload) {
  const totalPages = Math.max(1, Math.ceil(payload.totalSegments / payload.pageSize));
  insightsResults.innerHTML = `
    <div class="insights-diagnostics" title="Measured inside TalentInsightsService">
      <span>⏱ ${payload.diagnostics.elapsedMilliseconds} ms</span>
      <span>⇄ ${payload.diagnostics.databaseRoundTrips} DB round trips</span>
      <span>Σ ${payload.totalSegments} distinct segments</span>
    </div>
    ${payload.segments.length ? `<div class="insights-table-wrap"><table class="insights-table">
      <thead><tr><th>Skill</th><th>Location</th><th>Company / role</th><th>Contract</th><th>Candidates ↓</th><th>Sample</th></tr></thead>
      <tbody>${payload.segments.map(segmentRow).join("")}</tbody>
    </table></div>` : `<p class="no-results">No segments match. Loosen a filter or <a href="/insights">reset</a>.</p>`}
    <div class="pager">
      <button type="button" data-page="${payload.page - 1}" ${payload.page <= 1 ? "disabled" : ""}>← Prev</button>
      <span>Page ${payload.page} / ${totalPages}</span>
      <button type="button" data-page="${payload.page + 1}" ${payload.page >= totalPages ? "disabled" : ""}>Next →</button>
    </div>`;
  insightsResults.hidden = false;
}

async function exploreTalentMarket(page = 1, { updateHistory = true } = {}) {
  const pageQuery = insightsQueryFromForm(page);
  if (updateHistory) history.pushState(null, "", `/insights${pageQuery.size ? `?${pageQuery}` : ""}`);
  const apiQuery = new URLSearchParams(pageQuery);
  apiQuery.set("page", String(page));
  apiQuery.set("pageSize", String(insightsPageSize));
  insightsStatus.hidden = false;
  insightsStatus.textContent = "Crunching distinct segments… (check the console for SQL timings)";
  try {
    const response = await fetch(`/api/insights/talent-market?${apiQuery}`, { headers: { Accept: "application/json" } });
    if (!response.ok) throw new Error(`Request failed (${response.status}).`);
    renderInsights(await response.json());
    insightsStatus.hidden = true;
  } catch (error) {
    insightsStatus.textContent = `Could not load the talent market. ${error.message}`;
  }
}

async function loadInsights() {
  searchPage.hidden = true;
  insightsPage.hidden = false;
  document.title = "Talent market · Portfolio directory";
  const params = new URLSearchParams(window.location.search);
  insightsForm.elements.q.value = params.get("q") || "";
  insightsStatus.textContent = "Loading filters…";
  try {
    const response = await fetch("/api/insights/facets", { headers: { Accept: "application/json" } });
    if (!response.ok) throw new Error(`Request failed (${response.status}).`);
    const facets = await response.json();
    fillSelect(insightsForm.elements.category, facets.categories, params.get("category"));
    fillSelect(insightsForm.elements.location, facets.locations, params.get("location"));
    fillSelect(insightsForm.elements.employmentType, facets.employmentTypes, params.get("employmentType"));
    fillSelect(insightsForm.elements.workMode, facets.workModes, params.get("workMode"));
  } catch (error) {
    insightsStatus.textContent = `Could not load filters. ${error.message}`;
    return;
  }
  const page = Math.max(1, Number.parseInt(params.get("page") || "1", 10) || 1);
  void exploreTalentMarket(page, { updateHistory: false });
}

insightsForm.addEventListener("submit", event => {
  event.preventDefault();
  void exploreTalentMarket();
});
insightsFilterNames.forEach(name =>
  insightsForm.elements[name].addEventListener("change", () => void exploreTalentMarket()));
insightsResults.addEventListener("click", event => {
  const button = event.target.closest("button[data-page]");
  if (button && !button.disabled) void exploreTalentMarket(Number(button.dataset.page));
});

const routeMatch = window.location.pathname.match(/^\/profile\/([^/]+)\/?$/);
const projectRouteMatch = window.location.pathname.match(/^\/project\/([^/]+)\/?$/);
if (routeMatch) {
  let profileKey = routeMatch[1];
  try {
    profileKey = decodeURIComponent(profileKey);
  } catch {
    detailStatus.hidden = false;
    detailStatus.textContent = "The profile key in this URL is invalid.";
  }
  if (detailStatus.hidden) void loadProfile(profileKey);
} else if (projectRouteMatch) {
  let projectSlug = projectRouteMatch[1];
  try {
    projectSlug = decodeURIComponent(projectSlug);
    void loadProject(projectSlug);
  } catch {
    detailStatus.hidden = false;
    detailStatus.textContent = "The project key in this URL is invalid.";
  }
} else if (/^\/insights\/?$/.test(window.location.pathname)) {
  void loadInsights();
} else {
  loadSearchFromUrl();
}
