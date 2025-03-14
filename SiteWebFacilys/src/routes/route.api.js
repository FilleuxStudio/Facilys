const express = require("express");
const router = express.Router();
const apiController = require("../controllers/controller.api");

router.get("/version", apiController.getVersion);
router.post("/login", apiController.login);
router.post("/query", apiController.executeQuery);

module.exports = router;