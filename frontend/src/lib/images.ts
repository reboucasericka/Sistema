export const IMAGES = {
  logo: '/images/1.jpg',
  logoBar: '/images/home/logo_bar.png',
  logoBlack: '/images/home/logo_black.png',
  placeholder: '/images/placeholders/noimage.png',
  avatar: '/images/placeholders/user.png',
  loginBg: '/images/home/slider_home_3.png',
  banner: '/images/home/banner-produto.jpg',
  slider1: '/images/home/slider_home_3.png',
  slider2: '/images/home/slider_home_2.png',
  aboutFooter: '/images/About/about-img.jpg',
  lapisLiftDraw: '/images/home/lapis_lift_draw.jpg',
  pd1: '/images/home/pd1.jpg',
  pd2: '/images/home/pd2.jpg',
  pd3: '/images/home/pd3.jpg',
  video1: '/images/home/video_1.jpg',
  video3: '/images/home/video_3.jpg',
} as const

/** Resolve URL pública de imagem (storage API, legado /images, ou absoluta). */
export function mediaUrl(pathOrUrl?: string | null): string | null {
  if (!pathOrUrl) return null
  if (pathOrUrl.startsWith('http://') || pathOrUrl.startsWith('https://')) return pathOrUrl
  if (pathOrUrl.startsWith('/images/')) return pathOrUrl

  const base = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? ''

  if (pathOrUrl.startsWith('/storage/')) {
    return base ? `${base}${pathOrUrl}` : pathOrUrl
  }

  // Path relativo gerido pelo storage (ex.: professionals/uuid.jpg)
  if (pathOrUrl.includes('/') && !pathOrUrl.startsWith('/')) {
    return base ? `${base}/storage/${pathOrUrl}` : `/storage/${pathOrUrl}`
  }

  return null
}

export function serviceImage(imageUrl?: string | null, index = 0): string {
  const fromApi = mediaUrl(imageUrl)
  if (fromApi) return fromApi
  const pool = [
    IMAGES.pd1,
    IMAGES.pd2,
    IMAGES.pd3,
    '/images/home/product_1.png',
    '/images/home/product_2.png',
    '/images/home/product_3.png',
  ]
  return pool[index % pool.length]
}
