'use strict'

exports.config = {
  app_name: ['MAUI-Backend'],
  license_key: process.env.NEW_RELIC_LICENSE_KEY,
  distributed_tracing: {
    enabled: true
  },
  allow_all_headers: true,
  attributes: {
    exclude: [
      'request.headers.cookie',
      'request.headers.authorization',
      'request.headers.proxyAuthorization',
      'request.headers.setCookie*',
      'response.headers.cookie',
      'response.headers.authorization',
      'response.headers.proxyAuthorization',
      'response.headers.setCookie*'
    ]
  },
  application_logging: {
    forwarding: {
      enabled: true
    },
    metrics: {
      enabled: true
    },
    local_decorating: {
      enabled: false
    }
  },
  logging: {
    level: 'info'
  }
}
