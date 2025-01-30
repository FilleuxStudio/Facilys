const express = require("express");
const router = express.Router();

router.post('/api/version', (req, res) => {
        res.send('version 1');

});

router.post('/api/login', (req, res) => {
        res.json('version 1');
});

module.exports = router;