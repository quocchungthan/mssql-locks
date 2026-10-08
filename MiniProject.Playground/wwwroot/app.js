"use strict";

const portfolioRoot = document.querySelector("#portfolio");
const statusMessage = document.querySelector("#status");
const profileKeyInput = document.querySelector("#profile-key");
const profilePicker = document.querySelector("#profile-picker");

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
      <ul class="skill-list">${items.map(skill => `<li>${escapeHtml(skill.name)}</li>`).join("")}</ul>
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
        <h3>${escapeHtml(project.title)}</h3>
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
  const imageUrl = safeUrl(profile.profileImageUrl);
  const avatar = imageUrl
    ? `<img class="avatar" src="${escapeHtml(imageUrl)}" alt="" onerror="this.hidden=true">`
    : `<div class="avatar avatar-placeholder" aria-hidden="true">${escapeHtml(profile.displayName.slice(0, 1).toUpperCase())}</div>`;

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
  profilePicker.hidden = false;
  profileKeyInput.value = profileKey;
  statusMessage.hidden = false;
  portfolioRoot.hidden = true;
  statusMessage.textContent = "Loading profile…";

  try {
    const response = await fetch(`/api/portfolios/${encodeURIComponent(profileKey)}`, {
      headers: { Accept: "application/json" }
    });
    if (response.status === 404) {
      statusMessage.textContent = `No profile was found for “${profileKey}”.`;
      return;
    }
    if (!response.ok) throw new Error(`Request failed (${response.status}).`);
    renderProfile(await response.json());
    statusMessage.hidden = true;
    document.title = `${profileKeyInput.value} · Portfolio`;
  } catch (error) {
    statusMessage.textContent = `Could not load this profile. ${error.message}`;
  }
}

profilePicker.addEventListener("submit", event => {
  event.preventDefault();
  const profileKey = profileKeyInput.value.trim();
  if (!profileKey) return;
  window.location.assign(`/profile/${encodeURIComponent(profileKey)}`);
});

const routeMatch = window.location.pathname.match(/^\/profile\/([^/]+)\/?$/);
if (routeMatch) {
  let profileKey = routeMatch[1];
  let validProfileKey = true;
  try {
    profileKey = decodeURIComponent(profileKey);
  } catch {
    statusMessage.textContent = "The profile key in this URL is invalid.";
    profilePicker.hidden = true;
    validProfileKey = false;
  }
  if (validProfileKey) loadProfile(profileKey);
} else {
  profileKeyInput.focus();
}
