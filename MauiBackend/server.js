require('dotenv').config()
require('newrelic')

const express = require('express')
const winston = require('winston')

const logger = winston.createLogger({
  level: 'info',
  format: winston.format.simple(),
  transports: [new winston.transports.Console()]
})

const app = express()
const PORT = process.env.PORT || 3000

function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms))
}

app.get('/login', async (req, res) => {
  const userId = req.header('x-user-id')

  const delayMs = Math.floor(Math.random() * 3001)
  logger.info(`Login request received, sleeping for ${delayMs}ms`)
  await sleep(delayMs)

  if (!userId) {
    try {
      throw new Error('No user name supplied')
    } catch (err) {
      logger.error(`Login failed: ${err.message}`)
      return res.status(400).json({ message: 'Error no user name' })
    }
  }

  if (Math.random() < 0.9) {
    logger.info(`Login successful for user "${userId}"`)
    return res.status(200).json({ message: 'Login successful' })
  }

  logger.warn(`User not recognised: "${userId}"`)
  return res.status(401).json({ message: 'User not recognised' })
})

app.listen(PORT, () => {
  logger.info(`MAUI-Backend listening on port ${PORT}`)
})
