const express = require("express");
const router = express.Router();
const apiController = require("../controllers/controller.api");

router.get("/version", apiController.getVersion);
router.post("/login", apiController.login);
router.post("/getlogindatabse", apiController.loginDatabase);
router.post("/company", apiController.company);
router.post("/query", apiController.executeQuery);
router.put("/update-company", apiController.updateCompany);

// === INSERT (Ajout) ===
router.post("/query/addclient", apiController.executeQueryAddClient);
router.post("/query/addphone", apiController.executeQueryAddPhone);
router.post("/query/addemail", apiController.executeQueryAddEmail);
router.post("/query/addvehicle", apiController.executeQueryAddVehicle);
router.post("/query/addothervehicle", apiController.executeQueryAddOtherVehicle);
router.post("/query/addinvoice", apiController.executeQueryAddInvoice);

// === UPDATE (Mise à jour) ===
router.post("/query/updateclient", apiController.executeQueryUpdateClient);
router.post("/query/updatevehicle", apiController.executeQueryUpdateVehicle);
router.post("/query/updateothervehicle", apiController.executeQueryUpdateOtherVehicle);
router.post("/query/updateinvoice", apiController.executeQueryUpdateInvoice);

module.exports = router;