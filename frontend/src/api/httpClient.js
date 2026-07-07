import { API_BASE_URL } from "../config/apiConfig";

async function handleResponse(response) {
  const contentType = response.headers.get("content-type") || "";
  const isJson = contentType.includes("json");
  const data = isJson ? await response.json().catch(() => null) : null;

  if (!response.ok) {
    throw new Error(extractErrorMessage(data, response.status));
  }

  return data;
}

// ASP.NET Core returns ProblemDetails ({ title, detail }) for business errors
// and a validation problem ({ errors: { Field: ["message"] } }) for invalid input.
function extractErrorMessage(data, status) {
  if (!data) {
    return `Request failed with status ${status}.`;
  }

  if (data.errors) {
    const firstError = Object.values(data.errors).flat()[0];
    if (firstError) {
      return firstError;
    }
  }

  return data.detail || data.title || `Request failed with status ${status}.`;
}

export async function apiGet(path) {
  const response = await fetch(`${API_BASE_URL}${path}`);
  return handleResponse(response);
}

export async function apiPost(path, body) {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  return handleResponse(response);
}
