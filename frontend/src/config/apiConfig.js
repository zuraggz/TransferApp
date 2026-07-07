// Central place for API configuration. Override VITE_API_BASE_URL in .env
// (or as a build-time env var in Docker) to point at a different backend.
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5205/api";
