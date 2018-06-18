

-- Dumping structure for table saudi.gas_price
DROP TABLE IF EXISTS `gas_price`;
CREATE TABLE IF NOT EXISTS `gas_price` (
  `gsp_id` int(10) NOT NULL AUTO_INCREMENT,
  `gsp_gas_id` int(10) NOT NULL,
  `gsp_date` date DEFAULT NULL,
  `gsp_buyCost` decimal(10,3) NOT NULL,
  `gsp_sellCost` decimal(10,3) NOT NULL,
  `gsp_buy_tax` decimal(10,3) DEFAULT NULL,
  `gsp_sell_tax` decimal(10,3) DEFAULT NULL,
  PRIMARY KEY (`gsp_id`),
  KEY `FK_gas_price_gas` (`gsp_gas_id`),
  CONSTRAINT `FK_gas_price_gas` FOREIGN KEY (`gsp_gas_id`) REFERENCES `gas` (`gas_id`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8;

-- Dumping data for table saudi.gas_price: ~9 rows (approximately)
/*!40000 ALTER TABLE `gas_price` DISABLE KEYS */;
INSERT INTO `gas_price` (`gsp_id`, `gsp_gas_id`, `gsp_date`, `gsp_buyCost`, `gsp_sellCost`, `gsp_buy_tax`, `gsp_sell_tax`) VALUES
	(1, 1, '2012-01-01', 0.519, 0.600, 0.000, 0.000),
	(2, 2, '2012-01-01', 0.369, 0.450, 0.000, 0.000),
	(3, 3, '2012-01-01', 0.224, 0.250, 0.000, 0.000),
	(4, 1, '2016-01-01', 0.822, 0.900, 0.000, 0.000),
	(5, 2, '2016-01-01', 0.672, 0.750, 0.000, 0.000),
	(6, 3, '2016-01-01', 0.430, 0.450, 0.000, 0.000),
	(7, 1, '2018-01-01', 1.945, 2.040, 5.000, 5.000),
	(8, 2, '2018-01-01', 1.275, 1.370, 5.000, 5.000),
	(9, 3, '2018-01-01', 0.433, 0.470, 5.000, 5.000);
/*!40000 ALTER TABLE `gas_price` ENABLE KEYS */;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
