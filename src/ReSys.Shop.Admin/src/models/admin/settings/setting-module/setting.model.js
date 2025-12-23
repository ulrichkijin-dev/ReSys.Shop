// src/ReSys.Shop.Admin/src/models/admin/settings/setting-module/setting.model.js

/**
 * @enum {string} ConfigurationValueType
 * @property {string} String - The setting value is a string.
 * @property {string} Number - The setting value is a number.
 * @property {string} Boolean - The setting value is a boolean.
 * @property {string} Json - The setting value is a JSON object.
 */
export const ConfigurationValueType = {
  String: 'String',
  Number: 'Number',
  Boolean: 'Boolean',
  Json: 'Json',
};

/**
 * @typedef {object} SettingParameter
 * @property {string} key - The unique key of the setting.
 * @property {string} value - The current value of the setting.
 * @property {string} description - A description of the setting.
 * @property {string} defaultValue - The default value of the setting.
 * @property {ConfigurationValueType} valueType - The type of the setting's value.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */

/**
 * @typedef {object} SettingSelectItem
...
/**
 * @typedef {SettingListItem & object} SettingDetail
 * @property {string} defaultValue - The default value of the setting.
 * @property {Object.<string, any>} [publicMetadata] - Public metadata.
 * @property {Object.<string, any>} [privateMetadata] - Private metadata.
 */
