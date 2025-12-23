// src/ReSys.Shop.Admin/src/models/admin/catalog/products/images/product-image.model.js

/**
 * @typedef {object} ProductImageParameter
 * @property {string} type - The type of image (e.g., "default", "thumbnail").
 * @property {string} [url] - The URL of the image.
 * @property {string} [alt] - Alternative text for the image.
 * @property {number} position - The display order of the image.
 */

/**
 * @typedef {ProductImageParameter & object} ProductImageUploadParameter
 * @property {string} [variantId] - Optional: The ID of the variant this image belongs to.
 * @property {File | null} [file] - The image file to upload.
 */

/**
 * @typedef {object} ProductImageResult
 * @property {string} id - The unique identifier of the image.
 * @property {string} type - The type of image.
 * @property {string} url - The URL of the image.
 * @property {string} [alt] - Alternative text for the image.
 * @property {number} position - The display order of the image.
 * @property {number} size - The size of the image file in bytes.
 * @property {string} contentType - The content type of the image (e.g., "image/jpeg").
 * @property {number} [width] - The width of the image in pixels.
 * @property {number} [height] - The height of the image in pixels.
 * @property {Object.<number, string>} [thumbnails] - Dictionary of thumbnail sizes and their URLs.
 */
