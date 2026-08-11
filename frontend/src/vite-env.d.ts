/// <reference types="vite/client" />

import 'slick-carousel'

declare module 'jquery' {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  interface JQuery<TElement = HTMLElement> {
    slick(options?: object): this
    slick(method: 'unslick'): this
  }
}
