const BOOLEAN_KEYS = new Set([
  'is_active',
  'remove_photo',
  'remove_image',
  'low_stock',
  'unread_only',
])

/**
 * Constrói FormData para multipart (uploads).
 * Usa _method para spoofing PUT/PATCH quando necessário.
 */
export function toFormData(
  payload: Record<string, unknown>,
  options?: {
    method?: 'PUT' | 'PATCH'
    files?: Record<string, File | null | undefined>
  },
): FormData {
  const formData = new FormData()

  if (options?.method) {
    formData.append('_method', options.method)
  }

  for (const [key, value] of Object.entries(payload)) {
    if (value === undefined) continue

    if (value === null) {
      formData.append(key, '')
      continue
    }

    if (typeof value === 'boolean' || BOOLEAN_KEYS.has(key)) {
      formData.append(key, value === true || value === '1' || value === 1 ? '1' : '0')
      continue
    }

    if (typeof value === 'number') {
      formData.append(key, String(value))
      continue
    }

    formData.append(key, String(value))
  }

  if (options?.files) {
    for (const [key, file] of Object.entries(options.files)) {
      if (file) formData.append(key, file)
    }
  }

  return formData
}

export function needsMultipart(
  files: Record<string, File | null | undefined>,
  removeFlags: Record<string, boolean | undefined>,
): boolean {
  return (
    Object.values(files).some(Boolean) || Object.values(removeFlags).some(Boolean)
  )
}
