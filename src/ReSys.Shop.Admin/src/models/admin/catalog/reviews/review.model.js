// src/ReSys.Shop.Admin/src/models/admin/catalog/reviews/review.model.js

/**
 * @typedef {object} ReviewItem
 * @property {string} id - The unique identifier of the review.
 * @property {string} productId - The ID of the product the review is for.
 * @property {string} [productName] - The name of the product.
 * @property {string} userId - The ID of the user who submitted the review.
 * @property {string} [userName] - The username of the user.
 * @property {number} rating - The rating given in the review (e.g., 1-5).
 * @property {string} [title] - The title of the review.
 * @property {string} [comment] - The review comment.
 * @property {string} status - The current status of the review (e.g., "Pending", "Approved", "Rejected").
 * @property {string} createdAt - Date and time when the review was created (ISO 8601).
 * @property {string} [moderatedBy] - The ID of the moderator who last actioned the review.
 * @property {string} [moderatedAt] - Date and time when the review was last moderated (ISO 8601).
 * @property {string} [moderationNotes] - Notes from the moderator.
 */

/**
 * @typedef {object} ApproveReviewParameter
 * @property {string} [notes] - Optional notes from the moderator when approving.
 */

/**
 * @typedef {object} RejectReviewParameter
 * @property {string} reason - The reason for rejecting the review.
 */
