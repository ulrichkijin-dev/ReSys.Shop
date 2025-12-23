// src/ReSys.Shop.Admin/src/models/admin/promotions/promotion.model.js

/**
 * @enum {string} PromotionType
 * @property {string} OrderDiscount - Discount applies to the entire order.
 * @property {string} ItemDiscount - Discount applies to specific items.
 * @property {string} BuyXGetY - Buy X items and get Y items free/discounted.
 * // Add other types as discovered
 */
export const PromotionType = {
  OrderDiscount: 'OrderDiscount',
  ItemDiscount: 'ItemDiscount',
  BuyXGetY: 'BuyXGetY',
};

/**
 * @enum {string} DiscountType
 * @property {string} Percentage - Discount is a percentage of the value.
 * @property {string} FixedAmount - Discount is a fixed amount.
 * // Add other types as discovered
 */
export const DiscountType = {
  Percentage: 'Percentage',
  FixedAmount: 'FixedAmount',
};

/**
 * @enum {string} PromotionRuleType
 * @property {string} MinimumOrderAmount - Rule based on minimum order amount.
 * @property {string} ProductQuantity - Rule based on product quantity.
 * @property {string} HasProduct - Rule checks if order contains specific product.
 * @property {string} HasTaxon - Rule checks if order contains product from specific taxon.
 * @property {string} UserHasRole - Rule checks if user has specific role.
 * // Add other types as discovered
 */
export const PromotionRuleType = {
  MinimumOrderAmount: 'MinimumOrderAmount',
  ProductQuantity: 'ProductQuantity',
  HasProduct: 'HasProduct',
  HasTaxon: 'HasTaxon',
  UserHasRole: 'UserHasRole',
};


/**
 * @typedef {object} PromotionActionTransfer
 * @property {PromotionType} type - The type of promotion action.
 * @property {DiscountType} [discountType] - The type of discount (for OrderDiscount/ItemDiscount).
 * @property {number} [value] - The discount value.
 * @property {string} [buyVariantId] - The ID of the variant to buy (for BuyXGetY).
 * @property {number} [buyQuantity] - The quantity of the variant to buy (for BuyXGetY).
 * @property {string} [getVariantId] - The ID of the variant to get (for BuyXGetY).
 * @property {number} [getQuantity] - The quantity of the variant to get (for BuyXGetY).
 */

/**
 * @typedef {object} PromotionRuleParameter
 * @property {PromotionRuleType} type - The type of promotion rule.
 * @property {string} value - The value associated with the rule.
 */

/**
 * @typedef {object} PromotionParameter
 * @property {string} name - The name of the promotion.
 * @property {string} [promotionCode] - The promotional code.
 * @property {string} [description] - A description of the promotion.
 * @property {number} [minimumOrderAmount] - Minimum order amount to qualify for the promotion.
 * @property {number} [maximumDiscountAmount] - Maximum discount amount that can be applied.
 * @property {string} [startsAt] - Start date and time of the promotion (ISO 8601).
 * @property {string} [expiresAt] - Expiry date and time of the promotion (ISO 8601).
 * @property {number} [usageLimit] - Maximum number of times the promotion can be used.
 * @property {boolean} active - Whether the promotion is active.
 * @property {boolean} requiresCouponCode - Whether a coupon code is required to use the promotion.
 * @property {PromotionActionTransfer} action - The action details of the promotion.
 */

/**
 * @typedef {object} PromotionSelectItem
 * @property {string} id - The unique identifier of the promotion.
 * @property {string} name - The name of the promotion.
 * @property {string} [description] - A description of the promotion.
 * @property {boolean} active - Whether the promotion is active.
 */

/**
 * @typedef {object} PromotionListItem
 * @property {string} id - The unique identifier of the promotion.
 * @property {string} name - The name of the promotion.
 * @property {string} [promotionCode] - The promotional code.
 * @property {string} [description] - A description of the promotion.
 * @property {string} type - The type of promotion (e.g., "OrderDiscount").
 * @property {number} [minimumOrderAmount] - Minimum order amount.
 * @property {number} [maximumDiscountAmount] - Maximum discount amount.
 * @property {string} [startsAt] - Start date.
 * @property {string} [expiresAt] - Expiry date.
 * @property {number} [usageLimit] - Total usage limit.
 * @property {number} usageCount - Current usage count.
 * @property {boolean} active - Whether the promotion is active.
 * @property {boolean} isActive - Whether the promotion is currently active.
 * @property {boolean} isExpired - Whether the promotion has expired.
 * @property {number} remainingUsage - Remaining usage count.
 * @property {string} createdAt - Date and time when the promotion was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the promotion was last updated (ISO 8601).
 */

/**
 * @typedef {PromotionListItem & object} PromotionDetail
 * @property {boolean} requiresCouponCode - Whether a coupon code is required.
 * @property {PromotionActionTransfer} [action] - The action details of the promotion.
 * @property {number} ruleCount - Number of rules associated with the promotion.
 */

/**
 * @typedef {PromotionRuleParameter & object} PromotionRuleItem
 * @property {string} id - The unique identifier of the rule.
 * @property {number} taxonCount - Number of taxons associated with this rule.
 * @property {number} userCount - Number of users associated with this rule.
 * @property {string} createdAt - Date and time when the rule was created (ISO 8601).
 * @property {string} [updatedAt] - Date and time when the rule was last updated (ISO 8601).
 */

/**
 * @typedef {object} PromotionTaxonRuleParameter
 * @property {string} [id] - Optional ID for existing rule taxon.
 * @property {string} taxonId - The ID of the taxon.
 */

/**
 * @typedef {PromotionTaxonRuleParameter & object} PromotionTaxonRuleItem
 * @property {string} taxonName - The name of the taxon.
 * @property {string} createdAt - Date and time when the rule taxon was created (ISO 8601).
 */

/**
 * @typedef {object} PromotionUsersRuleParameter
 * @property {string} [id] - Optional ID for existing rule user.
 * @property {string} userId - The ID of the user.
 */

/**
 * @typedef {PromotionUsersRuleParameter & object} PromotionUsersRuleItem
 * @property {string} [userName] - The username.
 * @property {string} [userFullName] - The full name of the user.
 * @property {string} [userEmail] - The email of the user.
 * @property {string} [userPhone] - The phone number of the user.
 */

/**
 * @typedef {object} PromotionStatsResult
 * @property {string} promotionId - The ID of the promotion.
 * @property {string} name - The name of the promotion.
 * @property {number} totalUsageCount - Total number of times the promotion has been used.
 * @property {number} remainingUsage - Remaining usage count.
 * @property {number} totalDiscountGiven - Total discount amount given.
 * @property {number} averageDiscountPerOrder - Average discount per order.
 * @property {number} affectedOrdersCount - Number of orders affected by the promotion.
 * @property {number} totalRevenueImpact - Total revenue impact.
 * @property {string} [firstUsedAt] - Date and time when first used (ISO 8601).
 * @property {string} [lastUsedAt] - Date and time when last used (ISO 8601).
 * @property {Object.<string, number>} usageByDay - Daily usage count.
 * @property {PromotionTopProductItem[]} topAffectedProducts - Top products affected.
 * @property {PromotionPerformanceMetrics} performance - Performance metrics.
 */

/**
 * @typedef {object} PromotionTopProductItem
 * @property {string} productId - The ID of the product.
 * @property {string} productName - The name of the product.
 * @property {number} timesDiscounted - Number of times this product was discounted.
 * @property {number} totalDiscount - Total discount applied to this product.
 */

/**
 * @typedef {object} PromotionPerformanceMetrics
 * @property {number} conversionRate - Conversion rate.
 * @property {number} revenuePerUse - Revenue per use.
 * @property {number} costPerAcquisition - Cost per acquisition.
 * @property {number} returnOnInvestment - Return on investment.
 */

/**
 * @typedef {object} PromotionPreviewResult
 * @property {boolean} isApplicable - Whether the promotion is applicable.
 * @property {string} [reasonNotApplicable] - Reason why it's not applicable.
 * @property {PromotionPreviewAdjustment[]} adjustments - List of adjustments.
 * @property {number} totalDiscount - Total discount.
 * @property {number} originalTotal - Original total.
 * @property {number} finalTotal - Final total.
 * @property {string[]} ruleEvaluations - Rule evaluation messages.
 */

/**
 * @typedef {object} PromotionPreviewAdjustment
 * @property {string} description - Description of the adjustment.
 * @property {number} amount - Amount of the adjustment.
 * @property {string} [lineItemId] - ID of the affected line item.
 * @property {string} [lineItemName] - Name of the affected line item.
 */

/**
 * @typedef {object} PromotionHistoryItem
 * @property {string} id - The unique identifier of the history entry.
 * @property {string} action - The action performed.
 * @property {string} description - Description of the history event.
 * @property {string} [performedBy] - Who performed the action (ID).
 * @property {string} [performedByName] - Who performed the action (Name).
 * @property {Object.<string, any>} [changesBefore] - State before changes.
 * @property {Object.<string, any>} [changesAfter] - State after changes.
 * @property {string} timestamp - Date and time of the event (ISO 8601).
 */
