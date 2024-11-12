const express = require("express");
const router = express.Router();

router.post('/version', (req, res) => {
        res.send('version 1');

});

module.exports = router;