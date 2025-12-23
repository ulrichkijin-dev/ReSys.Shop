// src/ReSys.Shop.Admin/src/models/admin/dashboard.model.js

/**
 * @typedef {object} SalesMetrics
 * @property {number} totalRevenue
 * @property {number} monthlyRevenue
 * @property {number} dailyRevenue
 * @property {string} currency
 */

/**
 * @typedef {object} OrderMetrics
 * @property {number} totalOrders
 * @property {number} pendingOrders
 * @property {number} processingOrders
 * @property {number} completedOrders
 * @property {number} todayOrders
 */

/**
 * @typedef {object} CatalogMetrics
 * @property {number} totalProducts
 * @property {number} activeProducts
 * @property {number} outOfStockVariants
 * @property {number} pendingReviews
 */

/**
 * @typedef {object} CustomerMetrics
 * @property {number} totalCustomers
 * @property {number} newCustomersThisMonth
 */

/**
 * @typedef {object} DashboardSummary
 * @property {SalesMetrics} sales
 * @property {OrderMetrics} orders
 * @property {CatalogMetrics} catalog
 * @property {CustomerMetrics} customers
 */

/**
 * @typedef {object} RecentOrder
 * @property {string} id
 * @property {string} number
 * @property {string} customerName
 * @property {number} total
 * @property {string} state
 * @property {string} createdAt
 */

/**
 * @typedef {object} RecentReview
 * @property {string} id
 * @property {string} productName
 * @property {string} customerName
 * @property {number} rating
 * @property {string} createdAt
 */

/**
 * @typedef {object} InventoryAlert
 * @property {string} variantId
 * @property {string} sku
 * @property {string} productName
 * @property {number} quantityOnHand
 * @property {number} backorderLimit
 */

/**
 * @typedef {object} TimeSeriesData
 * @property {string} label
 * @property {number} value
 */

/**
 * @typedef {object} CategorySales
 * @property {string} categoryName
 * @property {number} revenue
 * @property {number} orderCount
 */

/**
 * @typedef {object} TopProduct
 * @property {string} id
 * @property {string} productName
 * @property {number} quantitySold
 * @property {number} revenue
 */

/**
 * @typedef {object} SalesAnalysis
 * @property {TimeSeriesData[]} revenueTrends
 * @property {CategorySales[]} salesByCategory
 * @property {TopProduct[]} topSellingProducts
 */

/**
 * @typedef {object} OrderTrend
 * @property {TimeSeriesData[]} orderVolume
 * @property {Object.<string, number>} ordersByStatus
 */
