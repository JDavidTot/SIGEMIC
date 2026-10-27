-- MySQL dump 10.13  Distrib 9.7.0, for Win64 (x86_64)
--
-- Host: localhost    Database: sigemic
-- ------------------------------------------------------
-- Server version	9.7.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Current Database: `sigemic`
--

CREATE DATABASE /*!32312 IF NOT EXISTS*/ `sigemic` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;

USE `sigemic`;

--
-- Table structure for table `auditoria`
--

DROP TABLE IF EXISTS `auditoria`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auditoria` (
  `id_auditoria` bigint unsigned NOT NULL AUTO_INCREMENT,
  `id_usuario` int unsigned DEFAULT NULL,
  `fecha_hora` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `accion` varchar(100) NOT NULL,
  `tabla_afectada` varchar(100) DEFAULT NULL,
  `registro_afectado` varchar(100) DEFAULT NULL,
  `direccion_ip` varchar(45) DEFAULT NULL,
  `descripcion` varchar(1000) DEFAULT NULL,
  `resultado` enum('EXITOSO','FALLIDO') NOT NULL DEFAULT 'EXITOSO',
  PRIMARY KEY (`id_auditoria`),
  KEY `idx_auditoria_usuario` (`id_usuario`),
  KEY `idx_auditoria_fecha` (`fecha_hora`),
  CONSTRAINT `fk_auditoria_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `bancos`
--

DROP TABLE IF EXISTS `bancos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `bancos` (
  `id_banco` int unsigned NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) NOT NULL,
  `numero_cuenta` varchar(50) NOT NULL,
  `tipo_cuenta` enum('MONETARIA','AHORRO') NOT NULL,
  `beneficiario` varchar(150) NOT NULL,
  `estado` enum('ACTIVO','INACTIVO') NOT NULL DEFAULT 'ACTIVO',
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_banco`),
  UNIQUE KEY `uq_banco_cuenta` (`nombre`,`numero_cuenta`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `clientes`
--

DROP TABLE IF EXISTS `clientes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `clientes` (
  `id_cliente` int unsigned NOT NULL AUTO_INCREMENT,
  `id_usuario` int unsigned NOT NULL,
  `codigo_cliente` varchar(30) NOT NULL,
  `dpi_cifrado` varbinary(512) NOT NULL,
  `dpi_hash` binary(32) NOT NULL,
  `fecha_nacimiento` date DEFAULT NULL,
  `direccion_cifrada` varbinary(1024) DEFAULT NULL,
  `telefono_cifrado` varbinary(512) DEFAULT NULL,
  `profesion` varchar(100) DEFAULT NULL,
  `lugar_trabajo` varchar(150) DEFAULT NULL,
  `ingreso_mensual` decimal(12,2) DEFAULT NULL,
  `fecha_registro` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `estado` enum('ACTIVO','INACTIVO','BLOQUEADO') NOT NULL DEFAULT 'ACTIVO',
  PRIMARY KEY (`id_cliente`),
  UNIQUE KEY `uq_clientes_usuario` (`id_usuario`),
  UNIQUE KEY `uq_clientes_codigo` (`codigo_cliente`),
  UNIQUE KEY `uq_clientes_dpi_hash` (`dpi_hash`),
  KEY `idx_clientes_codigo` (`codigo_cliente`),
  CONSTRAINT `fk_clientes_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_clientes_ingreso` CHECK (((`ingreso_mensual` is null) or (`ingreso_mensual` >= 0)))
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `creditos`
--

DROP TABLE IF EXISTS `creditos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `creditos` (
  `id_credito` bigint unsigned NOT NULL AUTO_INCREMENT,
  `codigo_credito` varchar(30) NOT NULL,
  `id_cliente` int unsigned NOT NULL,
  `id_plan` int unsigned NOT NULL,
  `monto_original` decimal(14,2) NOT NULL,
  `tipo_tasa_aplicada` enum('MENSUAL','ANUAL') NOT NULL,
  `porcentaje_interes_aplicado` decimal(7,4) NOT NULL,
  `plazo_meses` int unsigned NOT NULL,
  `cuota_original` decimal(14,2) NOT NULL,
  `saldo_capital` decimal(14,2) NOT NULL,
  `estado` enum('PENDIENTE','ACTIVO','ATRASADO','CANCELADO') NOT NULL DEFAULT 'PENDIENTE',
  `fecha_otorgamiento` date NOT NULL,
  `fecha_inicio` date NOT NULL,
  `fecha_finalizacion_estimada` date NOT NULL,
  `fecha_cancelacion` date DEFAULT NULL,
  PRIMARY KEY (`id_credito`),
  UNIQUE KEY `uq_creditos_codigo` (`codigo_credito`),
  KEY `fk_creditos_plan` (`id_plan`),
  KEY `idx_creditos_cliente` (`id_cliente`),
  KEY `idx_creditos_estado` (`estado`),
  KEY `idx_creditos_fecha` (`fecha_otorgamiento`),
  CONSTRAINT `fk_creditos_cliente` FOREIGN KEY (`id_cliente`) REFERENCES `clientes` (`id_cliente`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_creditos_plan` FOREIGN KEY (`id_plan`) REFERENCES `planes_credito` (`id_plan`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_creditos_cuota` CHECK ((`cuota_original` > 0)),
  CONSTRAINT `chk_creditos_fechas` CHECK ((`fecha_finalizacion_estimada` >= `fecha_inicio`)),
  CONSTRAINT `chk_creditos_interes` CHECK (((`porcentaje_interes_aplicado` > 0) and (`porcentaje_interes_aplicado` <= 100))),
  CONSTRAINT `chk_creditos_monto` CHECK ((`monto_original` > 0)),
  CONSTRAINT `chk_creditos_plazo` CHECK ((`plazo_meses` in (6,12,18,24,36,48,60))),
  CONSTRAINT `chk_creditos_saldo` CHECK (((`saldo_capital` >= 0) and (`saldo_capital` <= `monto_original`)))
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `cuotas`
--

DROP TABLE IF EXISTS `cuotas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `cuotas` (
  `id_cuota` bigint unsigned NOT NULL AUTO_INCREMENT,
  `id_credito` bigint unsigned NOT NULL,
  `numero_cuota` int unsigned NOT NULL,
  `fecha_vencimiento` date NOT NULL,
  `capital_programado` decimal(14,2) NOT NULL,
  `interes_programado` decimal(14,2) NOT NULL,
  `cuota_programada` decimal(14,2) NOT NULL,
  `capital_pagado` decimal(14,2) NOT NULL DEFAULT '0.00',
  `interes_pagado` decimal(14,2) NOT NULL DEFAULT '0.00',
  `mora_pagada` decimal(14,2) NOT NULL DEFAULT '0.00',
  `saldo_cuota` decimal(14,2) NOT NULL,
  `estado` enum('PENDIENTE','PARCIAL','PAGADA','ATRASADA') NOT NULL DEFAULT 'PENDIENTE',
  PRIMARY KEY (`id_cuota`),
  UNIQUE KEY `uq_cuotas_credito_numero` (`id_credito`,`numero_cuota`),
  KEY `idx_cuotas_credito` (`id_credito`),
  KEY `idx_cuotas_vencimiento` (`fecha_vencimiento`),
  KEY `idx_cuotas_estado` (`estado`),
  CONSTRAINT `fk_cuotas_credito` FOREIGN KEY (`id_credito`) REFERENCES `creditos` (`id_credito`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_cuotas_capital` CHECK ((`capital_programado` >= 0)),
  CONSTRAINT `chk_cuotas_interes` CHECK ((`interes_programado` >= 0)),
  CONSTRAINT `chk_cuotas_numero` CHECK ((`numero_cuota` > 0)),
  CONSTRAINT `chk_cuotas_pagado` CHECK (((`capital_pagado` >= 0) and (`interes_pagado` >= 0) and (`mora_pagada` >= 0))),
  CONSTRAINT `chk_cuotas_saldo` CHECK ((`saldo_cuota` >= 0)),
  CONSTRAINT `chk_cuotas_total` CHECK ((`cuota_programada` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=55 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `documentos_cliente`
--

DROP TABLE IF EXISTS `documentos_cliente`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `documentos_cliente` (
  `id_documento` bigint unsigned NOT NULL AUTO_INCREMENT,
  `id_cliente` int unsigned NOT NULL,
  `tipo_documento` enum('DPI','NIT','OTRO') NOT NULL,
  `numero_cifrado` varbinary(512) NOT NULL,
  `numero_hash` binary(32) NOT NULL,
  `fecha_emision` date DEFAULT NULL,
  `fecha_vencimiento` date DEFAULT NULL,
  `estado` enum('ACTIVO','INACTIVO') NOT NULL DEFAULT 'ACTIVO',
  PRIMARY KEY (`id_documento`),
  UNIQUE KEY `uq_documentos_hash` (`numero_hash`),
  KEY `fk_documentos_cliente` (`id_cliente`),
  CONSTRAINT `fk_documentos_cliente` FOREIGN KEY (`id_cliente`) REFERENCES `clientes` (`id_cliente`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_documentos_fechas` CHECK (((`fecha_vencimiento` is null) or (`fecha_emision` is null) or (`fecha_vencimiento` >= `fecha_emision`)))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `formas_pago`
--

DROP TABLE IF EXISTS `formas_pago`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `formas_pago` (
  `id_forma_pago` int unsigned NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `estado` enum('ACTIVO','INACTIVO') NOT NULL DEFAULT 'ACTIVO',
  PRIMARY KEY (`id_forma_pago`),
  UNIQUE KEY `uq_formas_pago_nombre` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `pagos`
--

DROP TABLE IF EXISTS `pagos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pagos` (
  `id_pago` bigint unsigned NOT NULL AUTO_INCREMENT,
  `id_credito` bigint unsigned NOT NULL,
  `id_cuota` bigint unsigned DEFAULT NULL,
  `id_usuario` int unsigned NOT NULL,
  `id_forma_pago` int unsigned NOT NULL,
  `id_banco` int unsigned DEFAULT NULL,
  `numero_boleta` varchar(100) DEFAULT NULL,
  `archivo_comprobante` varchar(255) DEFAULT NULL,
  `numero_recibo` varchar(40) NOT NULL,
  `tipo_operacion` enum('PAGO_CUOTA','ABONO_EXTRAORDINARIO') NOT NULL,
  `monto_total` decimal(14,2) NOT NULL,
  `capital_aplicado` decimal(14,2) NOT NULL DEFAULT '0.00',
  `interes_aplicado` decimal(14,2) NOT NULL DEFAULT '0.00',
  `mora_aplicada` decimal(14,2) NOT NULL DEFAULT '0.00',
  `fecha_pago` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `observacion` varchar(500) DEFAULT NULL,
  PRIMARY KEY (`id_pago`),
  UNIQUE KEY `uq_pagos_recibo` (`numero_recibo`),
  UNIQUE KEY `uq_pagos_banco_boleta` (`id_banco`,`numero_boleta`),
  KEY `fk_pagos_cuota` (`id_cuota`),
  KEY `fk_pagos_usuario` (`id_usuario`),
  KEY `fk_pagos_forma` (`id_forma_pago`),
  KEY `idx_pagos_credito` (`id_credito`),
  KEY `idx_pagos_fecha` (`fecha_pago`),
  CONSTRAINT `fk_pagos_banco` FOREIGN KEY (`id_banco`) REFERENCES `bancos` (`id_banco`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_pagos_credito` FOREIGN KEY (`id_credito`) REFERENCES `creditos` (`id_credito`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_pagos_cuota` FOREIGN KEY (`id_cuota`) REFERENCES `cuotas` (`id_cuota`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_pagos_forma` FOREIGN KEY (`id_forma_pago`) REFERENCES `formas_pago` (`id_forma_pago`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_pagos_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_pagos_distribucion` CHECK (((`capital_aplicado` >= 0) and (`interes_aplicado` >= 0) and (`mora_aplicada` >= 0))),
  CONSTRAINT `chk_pagos_monto` CHECK ((`monto_total` > 0)),
  CONSTRAINT `chk_pagos_suma` CHECK ((`monto_total` = ((`capital_aplicado` + `interes_aplicado`) + `mora_aplicada`)))
) ENGINE=InnoDB AUTO_INCREMENT=20 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `permisos`
--

DROP TABLE IF EXISTS `permisos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `permisos` (
  `id_permiso` int unsigned NOT NULL AUTO_INCREMENT,
  `codigo` varchar(80) NOT NULL,
  `nombre` varchar(120) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `modulo` varchar(50) NOT NULL,
  `estado` enum('ACTIVO','INACTIVO') NOT NULL DEFAULT 'ACTIVO',
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_permiso`),
  UNIQUE KEY `uq_permisos_codigo` (`codigo`)
) ENGINE=InnoDB AUTO_INCREMENT=14 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `planes_credito`
--

DROP TABLE IF EXISTS `planes_credito`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `planes_credito` (
  `id_plan` int unsigned NOT NULL AUTO_INCREMENT,
  `nombre` varchar(100) NOT NULL,
  `descripcion` varchar(255) DEFAULT NULL,
  `tipo_tasa` enum('MENSUAL','ANUAL') NOT NULL,
  `porcentaje_interes` decimal(7,4) NOT NULL,
  `estado` enum('ACTIVO','INACTIVO') NOT NULL DEFAULT 'ACTIVO',
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_plan`),
  UNIQUE KEY `uq_planes_credito_nombre` (`nombre`),
  CONSTRAINT `chk_planes_interes` CHECK (((`porcentaje_interes` > 0) and (`porcentaje_interes` <= 100)))
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `solicitudes_pago`
--

DROP TABLE IF EXISTS `solicitudes_pago`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `solicitudes_pago` (
  `id_solicitud` bigint unsigned NOT NULL AUTO_INCREMENT,
  `id_cliente` int unsigned NOT NULL,
  `id_credito` bigint unsigned NOT NULL,
  `id_cuota` bigint unsigned DEFAULT NULL,
  `id_banco` int unsigned NOT NULL,
  `id_usuario_revision` int unsigned DEFAULT NULL,
  `id_pago` bigint unsigned DEFAULT NULL,
  `tipo_operacion` enum('PAGO_CUOTA','ABONO_EXTRAORDINARIO') NOT NULL,
  `monto` decimal(14,2) NOT NULL,
  `numero_boleta` varchar(100) NOT NULL,
  `archivo_comprobante` varchar(255) NOT NULL,
  `estado` enum('PENDIENTE','APROBADA','RECHAZADA') NOT NULL DEFAULT 'PENDIENTE',
  `fecha_solicitud` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_revision` datetime DEFAULT NULL,
  `motivo_rechazo` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id_solicitud`),
  UNIQUE KEY `uq_solicitud_pago` (`id_pago`),
  KEY `fk_solicitud_cliente` (`id_cliente`),
  KEY `fk_solicitud_credito` (`id_credito`),
  KEY `fk_solicitud_cuota` (`id_cuota`),
  KEY `fk_solicitud_banco` (`id_banco`),
  KEY `fk_solicitud_usuario_revision` (`id_usuario_revision`),
  CONSTRAINT `fk_solicitud_banco` FOREIGN KEY (`id_banco`) REFERENCES `bancos` (`id_banco`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_cliente` FOREIGN KEY (`id_cliente`) REFERENCES `clientes` (`id_cliente`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_credito` FOREIGN KEY (`id_credito`) REFERENCES `creditos` (`id_credito`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_cuota` FOREIGN KEY (`id_cuota`) REFERENCES `cuotas` (`id_cuota`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_pago` FOREIGN KEY (`id_pago`) REFERENCES `pagos` (`id_pago`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_usuario_revision` FOREIGN KEY (`id_usuario_revision`) REFERENCES `usuarios` (`id_usuario`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_solicitud_monto` CHECK ((`monto` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usuario_permisos`
--

DROP TABLE IF EXISTS `usuario_permisos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuario_permisos` (
  `id_usuario` int unsigned NOT NULL,
  `id_permiso` int unsigned NOT NULL,
  `fecha_asignacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_usuario`,`id_permiso`),
  KEY `fk_usuario_permisos_permiso` (`id_permiso`),
  CONSTRAINT `fk_usuario_permisos_permiso` FOREIGN KEY (`id_permiso`) REFERENCES `permisos` (`id_permiso`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_usuario_permisos_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `id_usuario` int unsigned NOT NULL AUTO_INCREMENT,
  `nombres` varchar(100) NOT NULL,
  `apellidos` varchar(100) NOT NULL,
  `username` varchar(50) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `correo_cifrado` varbinary(512) DEFAULT NULL,
  `telefono_cifrado` varbinary(512) DEFAULT NULL,
  `rol` enum('ADMINISTRADOR','CAJERO','CLIENTE') NOT NULL,
  `estado` enum('ACTIVO','INACTIVO','BLOQUEADO') NOT NULL DEFAULT 'ACTIVO',
  `intentos_fallidos` tinyint unsigned NOT NULL DEFAULT '0',
  `ultimo_acceso` datetime DEFAULT NULL,
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_usuario`),
  UNIQUE KEY `uq_usuarios_username` (`username`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping routines for database 'sigemic'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-10-27  0:14:19
