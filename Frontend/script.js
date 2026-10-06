/* ============================================================
   GROWIA — Shared JavaScript Utilities
   Loaded by all pages: <script src="script.js"></script>
   ============================================================ */

'use strict';

/* ── API Base URL (change this once to update all pages) ──────── */
const API_BASE_URL = 'https://localhost:44303/api';

/* ── Device Fingerprint ─────────────────────────────────────── */
function getOrCreateDeviceId() {
  let id = localStorage.getItem('grawiaa_device_id');
  if (!id) {
    id = 'dev-' + Math.random().toString(36).substring(2, 15) +
                  Math.random().toString(36).substring(2, 15);
    localStorage.setItem('grawiaa_device_id', id);
  }
  return id;
}

/* ── Validation helpers ─────────────────────────────────────── */
function isValidEmail(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

function isValidPassword(pass) {
  return pass.length >= 6;
}

/* ── Form helpers ───────────────────────────────────────────── */
function showFieldError(fieldId, message) {
  const field = document.getElementById(fieldId);
  const err   = document.getElementById('error-' + fieldId);
  
  if (field) {
    field.style.borderColor = '#ff4d4d'; // تنوير الخانة بالأحمر
    field.classList.add('error');
  }
  
  if (err) {
    err.textContent = message;
    err.style.color = '#ff4d4d';
    err.style.display = 'block';
  }
}

// تعديل دالة المسح برضه عشان تشيل اللون الأحمر
function clearFormErrors() {
  document.querySelectorAll('.error-msg').forEach(el => el.textContent = '');
  document.querySelectorAll('.form-input').forEach(el => {
    el.classList.remove('error');
    el.style.borderColor = '#e2e8f0'; // ترجع للون الطبيعي
  });
}

function showBanner(bannerId, message) {
  const el = document.getElementById(bannerId);
  if (!el) return;
  el.textContent = message;
  el.style.display = 'block';
}

function hideBanner(bannerId) {
  const el = document.getElementById(bannerId);
  if (el) el.style.display = 'none';
}

/* ── Password visibility toggle ─────────────────────────────── */
function togglePass(inputId) {
  const input = document.getElementById(inputId);
  if (!input) return;
  input.type = input.type === 'password' ? 'text' : 'password';
}

/* ── Modal helpers ──────────────────────────────────────────── */
function openModal(id) {
  const el = document.getElementById(id);
  if (el) el.classList.add('active');
}

function closeModal(id) {
  const el = document.getElementById(id);
  if (el) el.classList.remove('active');
}

/* Close any open modal on Escape */
document.addEventListener('keydown', e => {
  if (e.key === 'Escape') {
    document.querySelectorAll('.modal-overlay.active').forEach(m => m.classList.remove('active'));
  }
});

/* Close modal when clicking the overlay backdrop (not the modal itself) */
document.addEventListener('click', e => {
  if (e.target.classList.contains('modal-overlay')) {
    e.target.classList.remove('active');
  }
});

/* ── Auth token helpers ─────────────────────────────────────── */
function getToken()         { return localStorage.getItem('token'); }
function setToken(token)    { localStorage.setItem('token', token); }
function clearToken()       { localStorage.removeItem('token'); }

function requireAuth(redirectTo) {
  if (!getToken()) {
    window.location.href = redirectTo || 'index.html';
    return false;
  }
  return true;
}

/* ── Logout ─────────────────────────────────────────────────── */
function logout(redirectTo) {
  localStorage.removeItem('token');
  localStorage.removeItem('adminToken');
  localStorage.removeItem('parentEmail');
  window.location.href = redirectTo || 'index.html';
}

