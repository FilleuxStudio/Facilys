const express = require("express");
const router = express.Router();
const apiController = require("../controllers/controller.api");

router.get("/version", apiController.getVersion);
router.post("/login", apiController.login);
router.post("/getlogindatabse", apiController.loginDatabase);
router.post("/company", apiController.company);
router.post("/query", apiController.executeQuery);
router.put("/update-company", apiController.updateCompany);

module.exports = router;