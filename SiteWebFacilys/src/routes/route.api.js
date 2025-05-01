const express = require("express");
const router = express.Router();
const apiController = require("../controllers/controller.api");

router.get("/version", apiController.getVersion);
router.post("/login", apiController.login);
router.post("/getlogindatabse", apiController.loginDatabase);
router.post("/company", apiController.company);
router.post("/query", apiController.executeQuery);
router.post("/query/addclient", apiController.executeQueryAddClient);
router.post("/query/addphone", apiController.executeQueryAddPhone);
router.post("/query/addemail", apiController.executeQueryAddEmail);
router.put("/update-company", apiController.updateCompany);

module.exports = router;