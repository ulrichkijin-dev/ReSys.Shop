// src/ReSys.Shop.Admin/src/services/notification/primevue.notification.service.js

/**
 * @typedef {import('./notification.interface').INotificationService} INotificationService
 */

/**
 * PrimeVue Toast implementation of INotificationService.
 * @implements {INotificationService}
 */
class PrimeVueNotificationService {
  /**
   * @private
   * @type {import('primevue/toast').ToastServiceMethods}
   */
  _toast;

  /**
   * @param {import('primevue/toast').ToastServiceMethods} toastInstance - The PrimeVue $toast instance.
   */
  constructor(toastInstance) {
    if (!toastInstance) {
      console.warn('PrimeVue $toast instance not provided to PrimeVueNotificationService. Notifications will not display.');
    }
    this._toast = toastInstance;
  }

  /**
   * Displays a success notification.
   * @param {string} detail - The message detail.
   * @param {string} [summary='Success'] - The message summary.
   * @param {number} [life=3000] - Duration in ms.
   */
  success(detail, summary = 'Success', life = 3000) {
    if (this._toast) {
      this._toast.add({ severity: 'success', summary, detail, life });
    } else {
      console.log('SUCCESS:', summary, detail);
    }
  }

  /**
   * Displays an info notification.
   * @param {string} detail - The message detail.
   * @param {string} [summary='Info'] - The message summary.
   * @param {number} [life=3000] - Duration in ms.
   */
  info(detail, summary = 'Info', life = 3000) {
    if (this._toast) {
      this._toast.add({ severity: 'info', summary, detail, life });
    } else {
      console.log('INFO:', summary, detail);
    }
  }

  /**
   * Displays a warning notification.
   * @param {string} detail - The message detail.
   * @param {string} [summary='Warning'] - The message summary.
   * @param {number} [life=3000] - Duration in ms.
   */
  warn(detail, summary = 'Warning', life = 3000) {
    if (this._toast) {
      this._toast.add({ severity: 'warn', summary, detail, life });
    } else {
      console.warn('WARN:', summary, detail);
    }
  }

  /**
   * Displays an error notification.
   * @param {string} detail - The message detail.
   * @param {string} [summary='Error'] - The message summary.
   * @param {number} [life=5000] - Duration in ms.
   */
  error(detail, summary = 'Error', life = 5000) {
    if (this._toast) {
      this._toast.add({ severity: 'error', summary, detail, life });
    } else {
      console.error('ERROR:', summary, detail);
    }
  }
}

export { PrimeVueNotificationService };
