const codeForm = document.querySelector("#codeForm");
const downloadBtn = document.querySelector("#downloadBtn");
const tabs = document.querySelectorAll(".tab");
const codeList = document.querySelector("#codeList");
const template = document.querySelector("#codeCardTemplate");

const API_BASE = window.BACKEND_URL ?? "http://localhost:5273";

let codes = [];
let activeLanguage = "python";
let hubConnection = null;

init();

async function init() {
  ensureBackendConfigured();
  bindEvents();
  await loadCodes();
  render();
  await setupRealtime();
}

function ensureBackendConfigured() {
  if (!API_BASE) {
    throw new Error("Thiếu cấu hình BACKEND_URL.");
  }
}

function bindEvents() {
  tabs.forEach((tab) => {
    tab.addEventListener("click", () => {
      tabs.forEach((btn) => btn.classList.remove("active"));
      tab.classList.add("active");
      activeLanguage = tab.dataset.tab;
      render();
    });
  });

  codeForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const title = document.querySelector("#titleInput").value.trim();
    const language = document.querySelector("#languageInput").value;
    const content = document.querySelector("#codeInput").value;

    if (!title || !content) {
      alert("Vui lòng điền đủ thông tin.");
      return;
    }

    try {
      await fetch(`${API_BASE}/api/codes`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ title, language, content }),
      });
      codeForm.reset();
    } catch (error) {
      console.error(error);
      alert("Không thể lưu code. Kiểm tra backend.");
    }
  });

  downloadBtn.addEventListener("click", () => {
    const blob = new Blob([JSON.stringify(codes, null, 2)], {
      type: "application/json",
    });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = "codes.json";
    anchor.click();
    URL.revokeObjectURL(url);
  });
}

async function loadCodes() {
  try {
    const response = await fetch(`${API_BASE}/api/codes`, {
      cache: "no-store",
    });
    if (!response.ok) {
      throw new Error("Không thể tải dữ liệu backend.");
    }
    codes = await response.json();
  } catch (error) {
    console.error(error);
    alert("Không thể tải danh sách code.");
  }
}

function render() {
  codeList.innerHTML = "";
  const filtered = codes.filter((code) => code.language === activeLanguage);

  if (!filtered.length) {
    codeList.innerHTML =
      '<p class="hint">Chưa có code cho mục này. Dùng form bên trái để thêm.</p>';
    return;
  }

  filtered.forEach((entry) => {
    const clone = template.content.cloneNode(true);
    clone.querySelector(".code-title").textContent = entry.title;
    clone.querySelector(".lang-tag").textContent = entry.language.toUpperCase();
    clone.querySelector("code").textContent = entry.content;
    const pre = clone.querySelector(".code-block");
    const viewBtn = clone.querySelector(".viewBtn");
    const deleteBtn = clone.querySelector(".deleteBtn");

    viewBtn.addEventListener("click", () => {
      pre.classList.toggle("hidden");
      viewBtn.textContent = pre.classList.contains("hidden") ? "Xem" : "Ẩn";
    });

    deleteBtn.addEventListener("click", async () => {
      if (!confirm(`Xóa "${entry.title}"?`)) return;
      try {
        const response = await fetch(`${API_BASE}/api/codes/${entry.id}`, {
          method: "DELETE",
        });
        if (response.status === 404) {
          alert("Code đã bị xóa trước đó.");
        }
      } catch (error) {
        console.error(error);
        alert("Không thể xóa code. Kiểm tra backend.");
      }
    });

    codeList.appendChild(clone);
  });
}

async function setupRealtime() {
  if (!window.signalR) {
    console.warn("SignalR client chưa được load.");
    return;
  }

  hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_BASE}/hub/codes`)
    .withAutomaticReconnect()
    .build();

  hubConnection.on("codesUpdated", async () => {
    await loadCodes();
    render();
  });

  try {
    await hubConnection.start();
  } catch (error) {
    console.error("Không thể kết nối SignalR", error);
  }
}

