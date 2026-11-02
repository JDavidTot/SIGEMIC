CREATE DATABASE  IF NOT EXISTS `sigemic` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `sigemic`;
-- MySQL dump 10.13  Distrib 8.0.46, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: sigemic
-- ------------------------------------------------------
-- Server version	9.7.0

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
SET @MYSQLDUMP_TEMP_LOG_BIN = @@SESSION.SQL_LOG_BIN;
SET @@SESSION.SQL_LOG_BIN= 0;

--
-- GTID state at the beginning of the backup 
--

SET @@GLOBAL.GTID_PURGED=/*!80000 '+'*/ '97098157-53be-11f1-8a08-049226c27c0a:1-547';

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
) ENGINE=InnoDB AUTO_INCREMENT=115 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `auditoria`
--

LOCK TABLES `auditoria` WRITE;
/*!40000 ALTER TABLE `auditoria` DISABLE KEYS */;
INSERT INTO `auditoria` VALUES (1,1,'2026-10-26 22:15:06','LOGIN_EXITOSO','usuarios','1','::1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(2,1,'2026-10-26 22:25:56','CLIENTE_EDITADO','clientes','2','::1','Cliente CLI-000003 actualizado. Usuario: Almatot.','EXITOSO'),(3,1,'2026-10-26 22:26:48','CLIENTE_ESTADO_CAMBIADO','clientes','1','::1','Cliente CLI-000002: estado cambiado de ACTIVO a INACTIVO.','EXITOSO'),(4,1,'2026-10-26 22:27:04','CLIENTE_ESTADO_CAMBIADO','clientes','1','::1','Cliente CLI-000002: estado cambiado de INACTIVO a ACTIVO.','EXITOSO'),(5,1,'2026-10-26 22:27:15','CLIENTE_EDITADO','clientes','4','::1','Cliente CLI-000008 actualizado. Usuario: estelac.','EXITOSO'),(6,1,'2026-10-26 22:33:25','CLIENTE_EDITADO','clientes','1','::1','Cliente CLI-000002 actualizado. Cambios: Profesi√≥n: Bachiller Industrial ‚Üí Ingeniero en Sistemas; Ingreso mensual: Q5,000.00 ‚Üí Q15,000.00.','EXITOSO'),(7,1,'2026-10-26 22:34:10','CLIENTE_ESTADO_CAMBIADO','clientes','3','::1','Cliente CLI-000005 actualizado. Cambios: Estado: ACTIVO ‚Üí INACTIVO.','EXITOSO'),(8,1,'2026-10-26 22:34:12','CLIENTE_ESTADO_CAMBIADO','clientes','3','::1','Cliente CLI-000005 actualizado. Cambios: Estado: INACTIVO ‚Üí ACTIVO.','EXITOSO'),(9,1,'2026-10-26 22:36:12','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(10,1,'2026-10-26 22:36:16','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(11,1,'2026-10-26 22:39:19','CREDITO_CREADO','creditos','4','127.0.0.1','Cr√©dito CR-000004 creado. Cliente: CLI-000008 - Estela Elizabeth Cu; Plan: Microcr√©dito Personal Anual; Monto: Q25,000.00; Tasa: 18.0000% ANUAL; Plazo: 36 meses; Cuota inicial: Q903.81; Saldo inicial: Q25,000.00; Estado: ACTIVO.','EXITOSO'),(12,1,'2026-10-26 22:46:40','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(13,4,'2026-10-26 22:47:16','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(14,4,'2026-10-26 22:47:29','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(15,1,'2026-10-26 22:47:33','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(16,1,'2026-10-26 22:47:53','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(17,4,'2026-10-26 22:47:55','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(18,4,'2026-10-26 22:49:08','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(19,1,'2026-10-26 22:49:11','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(20,1,'2026-10-26 23:01:59','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(21,4,'2026-10-26 23:02:02','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(22,4,'2026-10-26 23:02:44','PAGO_REGISTRADO','pagos','17','127.0.0.1','Pago REC-000017 registrado. Cr√©dito: CR-000004; Cliente: CLI-000008; Cuota: #2; Forma de pago: EFECTIVO; Monto: Q903.81; Capital aplicado: Q536.74; Inter√©s aplicado: Q367.07; Mora aplicada: Q0.00. Cambios: Saldo de cuota: Q903.81 ‚Üí Q0.00; Estado de cuota: PENDIENTE ‚Üí PAGADA; Saldo capital: Q24,471.19 ‚Üí Q23,934.45; Estado del cr√©dito: ACTIVO ‚Üí ACTIVO.','EXITOSO'),(23,4,'2026-10-26 23:03:04','PAGO_REGISTRADO','pagos','18','127.0.0.1','Pago REC-000018 registrado. Cr√©dito: CR-000003; Cliente: CLI-000005; Cuota: #9; Forma de pago: EFECTIVO; Monto: Q369.48; Capital aplicado: Q364.02; Inter√©s aplicado: Q5.46; Mora aplicada: Q0.00. Cambios: Saldo de cuota: Q369.48 ‚Üí Q0.00; Estado de cuota: PENDIENTE ‚Üí PAGADA; Saldo capital: Q364.02 ‚Üí Q0.00; Estado del cr√©dito: ACTIVO ‚Üí CANCELADO.','EXITOSO'),(24,4,'2026-10-26 23:03:08','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(25,1,'2026-10-26 23:03:13','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(26,1,'2026-10-26 23:12:43','CAJERO_EDITADO','usuarios','4','127.0.0.1','Cajero cajero01 actualizado. Cambios: Apellidos: SIGEMIC ‚Üí 01; Correo: Sin dato ‚Üí Dato actualizado.','EXITOSO'),(27,1,'2026-10-26 23:15:16','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(28,1,'2026-10-26 23:15:18','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(29,1,'2026-10-26 23:16:21','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(30,8,'2026-10-26 23:16:24','LOGIN_EXITOSO','usuarios','8','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: estelac. Rol: CLIENTE.','EXITOSO'),(31,8,'2026-10-26 23:48:59','LOGOUT','usuarios','8','127.0.0.1','Cierre de sesi√≥n del usuario: estelac.','EXITOSO'),(32,4,'2026-10-26 23:49:02','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(33,4,'2026-10-26 23:49:56','PAGO_REGISTRADO','pagos','19','127.0.0.1','Pago REC-000019 registrado. Cr√©dito: CR-000002; Cliente: CLI-000002; Cuota: #1; Forma de pago: DEPOSITO; Monto: Q877.63; Capital aplicado: Q802.63; Inter√©s aplicado: Q75.00; Mora aplicada: Q0.00; Banco ID: 1; Boleta: 123456; Cambios: Saldo de cuota: Q877.63 ‚Üí Q0.00; Estado de cuota: PENDIENTE ‚Üí PAGADA; Saldo capital: Q5,000.00 ‚Üí Q4,197.37; Estado del cr√©dito: ACTIVO ‚Üí ACTIVO.','EXITOSO'),(34,4,'2026-10-26 23:50:47','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(35,1,'2026-10-26 23:50:52','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(36,1,'2026-10-26 23:56:10','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(37,8,'2026-10-26 23:56:13','LOGIN_EXITOSO','usuarios','8','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: estelac. Rol: CLIENTE.','EXITOSO'),(38,8,'2026-10-26 23:58:57','LOGOUT','usuarios','8','127.0.0.1','Cierre de sesi√≥n del usuario: estelac.','EXITOSO'),(39,4,'2026-10-26 23:59:01','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(40,4,'2026-10-27 00:02:58','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(41,4,'2026-10-27 00:03:01','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(42,4,'2026-10-27 00:03:51','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(43,8,'2026-10-27 00:03:54','LOGIN_EXITOSO','usuarios','8','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: estelac. Rol: CLIENTE.','EXITOSO'),(44,8,'2026-10-27 00:05:51','LOGOUT','usuarios','8','127.0.0.1','Cierre de sesi√≥n del usuario: estelac.','EXITOSO'),(45,4,'2026-10-27 00:05:53','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(46,4,'2026-10-27 00:06:18','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(47,1,'2026-10-27 00:06:21','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(48,1,'2026-10-28 01:43:53','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(49,1,'2026-10-28 01:46:14','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(50,1,'2026-10-28 01:46:24','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(51,1,'2026-10-28 02:03:36','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(52,1,'2026-10-28 02:07:21','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(53,1,'2026-10-28 02:07:24','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(54,8,'2026-10-28 02:07:35','LOGIN_EXITOSO','usuarios','8','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: estelac. Rol: CLIENTE.','EXITOSO'),(55,8,'2026-10-28 02:12:53','LOGOUT','usuarios','8','127.0.0.1','Cierre de sesi√≥n del usuario: estelac.','EXITOSO'),(56,1,'2026-10-28 02:14:38','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(57,1,'2026-10-28 02:27:24','CLIENTE_EDITADO','clientes','1','127.0.0.1','Cliente CLI-000002 actualizado. Cambios: Correo: Sin dato ‚Üí Dato actualizado.','EXITOSO'),(58,1,'2026-10-28 02:40:32','PERFIL_ADMIN_ACTUALIZADO','usuarios','1','127.0.0.1','Perfil del administrador admin actualizado. Cambios: Correo: Sin dato ‚Üí Dato actualizado.','EXITOSO'),(59,1,'2026-10-28 02:40:51','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(60,4,'2026-10-28 02:40:54','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(61,4,'2026-10-28 02:41:03','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(62,8,'2026-10-28 02:41:05','LOGIN_EXITOSO','usuarios','8','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: estelac. Rol: CLIENTE.','EXITOSO'),(63,8,'2026-10-28 02:51:12','LOGOUT','usuarios','8','127.0.0.1','Cierre de sesi√≥n del usuario: estelac.','EXITOSO'),(64,1,'2026-10-28 02:55:28','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(65,1,'2026-10-28 03:04:34','USUARIO_CREADO','usuarios','9','127.0.0.1','Usuario josetot creado; Nombre: Jose Tot; Rol: ADMINISTRADOR; Correo: Registrado; Estado: ACTIVO; Acceso: Completo.','EXITOSO'),(66,1,'2026-10-28 03:04:39','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(67,9,'2026-10-28 03:04:42','LOGIN_EXITOSO','usuarios','9','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: josetot. Rol: ADMINISTRADOR.','EXITOSO'),(68,9,'2026-10-28 03:04:45','LOGOUT','usuarios','9','127.0.0.1','Cierre de sesi√≥n del usuario: josetot.','EXITOSO'),(69,9,'2026-10-28 03:04:48','LOGIN_EXITOSO','usuarios','9','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: josetot. Rol: ADMINISTRADOR.','EXITOSO'),(70,9,'2026-10-28 03:06:28','LOGOUT','usuarios','9','127.0.0.1','Cierre de sesi√≥n del usuario: josetot.','EXITOSO'),(71,4,'2026-10-28 03:06:31','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(72,4,'2026-10-28 03:06:40','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(73,8,'2026-10-28 03:06:43','LOGIN_EXITOSO','usuarios','8','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: estelac. Rol: CLIENTE.','EXITOSO'),(74,8,'2026-10-28 03:15:26','LOGOUT','usuarios','8','127.0.0.1','Cierre de sesi√≥n del usuario: estelac.','EXITOSO'),(75,9,'2026-10-28 03:15:31','LOGIN_EXITOSO','usuarios','9','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: josetot. Rol: ADMINISTRADOR.','EXITOSO'),(76,9,'2026-10-28 03:15:45','LOGOUT','usuarios','9','127.0.0.1','Cierre de sesi√≥n del usuario: josetot.','EXITOSO'),(77,4,'2026-10-28 03:15:49','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(78,1,'2026-11-01 20:23:03','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(79,1,'2026-11-01 20:23:15','SOLICITUD_APROBADA','solicitudes_pago','4','127.0.0.1','Solicitud #4 aprobada. Cliente: CLI-000008 - Estela Elizabeth Cu; Cr√©dito: CR-000004; Operaci√≥n: PAGO_CUOTA; Cuota #3; Monto: Q500.00; Banco: Banrural; Boleta: 1234567; Recibo generado: REC-000020. Cambios: Estado: PENDIENTE ‚Üí APROBADA; Pago asociado: Sin dato ‚Üí 20.','EXITOSO'),(80,1,'2026-11-01 20:46:39','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(81,1,'2026-11-01 20:47:01','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(82,1,'2026-11-01 20:47:19','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(83,1,'2026-11-01 20:47:25','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(84,1,'2026-11-01 20:47:34','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(85,1,'2026-11-01 20:49:07','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(86,1,'2026-11-01 20:54:33','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(87,2,'2026-11-01 21:13:58','LOGIN_EXITOSO','usuarios','2','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: Davidtot. Rol: CLIENTE.','EXITOSO'),(88,2,'2026-11-01 23:42:42','LOGOUT','usuarios','2','127.0.0.1','Cierre de sesi√≥n del usuario: Davidtot.','EXITOSO'),(89,2,'2026-11-01 23:42:44','LOGIN_EXITOSO','usuarios','2','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: Davidtot. Rol: CLIENTE.','EXITOSO'),(90,2,'2026-11-01 23:42:50','LOGOUT','usuarios','2','127.0.0.1','Cierre de sesi√≥n del usuario: Davidtot.','EXITOSO'),(91,1,'2026-11-01 23:42:54','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(92,1,'2026-11-02 00:22:21','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(93,4,'2026-11-02 00:22:26','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(94,4,'2026-11-02 00:22:46','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(95,1,'2026-11-02 00:22:50','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(96,1,'2026-11-02 00:23:20','PERMISOS_MODIFICADOS','usuario_permisos','4','127.0.0.1','Permisos del cajero cajero01 actualizados. Cambios: Permisos retirados: SOLICITUDES_APROBAR ‚Üí Sin dato.','EXITOSO'),(97,1,'2026-11-02 00:23:23','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(98,1,'2026-11-02 00:23:30','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(99,1,'2026-11-02 00:23:40','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(100,4,'2026-11-02 00:23:45','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(101,4,'2026-11-02 00:24:36','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(102,4,'2026-11-02 00:24:40','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(103,4,'2026-11-02 00:24:47','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(104,1,'2026-11-02 00:24:51','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(105,1,'2026-11-02 00:25:01','PERMISOS_MODIFICADOS','usuario_permisos','4','127.0.0.1','Permisos del cajero cajero01 actualizados. Cambios: Permisos retirados: SOLICITUDES_RECHAZAR, SOLICITUDES_VER ‚Üí Sin dato.','EXITOSO'),(106,1,'2026-11-02 00:25:04','LOGOUT','usuarios','1','127.0.0.1','Cierre de sesi√≥n del usuario: admin.','EXITOSO'),(107,4,'2026-11-02 00:25:07','LOGIN_EXITOSO','usuarios','4','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: cajero01. Rol: CAJERO.','EXITOSO'),(108,4,'2026-11-02 00:30:59','LOGOUT','usuarios','4','127.0.0.1','Cierre de sesi√≥n del usuario: cajero01.','EXITOSO'),(109,6,'2026-11-02 01:44:30','LOGIN_EXITOSO','usuarios','6','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: Victor. Rol: CAJERO.','EXITOSO'),(110,6,'2026-11-02 01:44:41','LOGOUT','usuarios','6','127.0.0.1','Cierre de sesi√≥n del usuario: Victor.','EXITOSO'),(111,8,'2026-11-02 01:44:46','LOGIN_EXITOSO','usuarios','8','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: estelac. Rol: CLIENTE.','EXITOSO'),(112,8,'2026-11-02 01:56:44','LOGOUT','usuarios','8','127.0.0.1','Cierre de sesi√≥n del usuario: estelac.','EXITOSO'),(113,1,'2026-11-02 01:56:50','LOGIN_EXITOSO','usuarios','1','127.0.0.1','Inicio de sesi√≥n exitoso. Usuario: admin. Rol: ADMINISTRADOR.','EXITOSO'),(114,1,'2026-11-02 01:57:58','SOLICITUD_APROBADA','solicitudes_pago','5','127.0.0.1','Solicitud #5 aprobada. Cliente: CLI-000008 - Estela Elizabeth Cu; Cr√©dito: CR-000004; Operaci√≥n: PAGO_CUOTA; Cuota #3; Monto: Q403.81; Banco: Banco Industrial; Boleta: 123ABC; Recibo generado: REC-000021. Cambios: Estado: PENDIENTE ‚Üí APROBADA; Pago asociado: Sin dato ‚Üí 21.','EXITOSO');
/*!40000 ALTER TABLE `auditoria` ENABLE KEYS */;
UNLOCK TABLES;

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
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `bancos`
--

LOCK TABLES `bancos` WRITE;
/*!40000 ALTER TABLE `bancos` DISABLE KEYS */;
INSERT INTO `bancos` VALUES (1,'Banco Industrial','1234567890','MONETARIA','SIGEMIC','ACTIVO','2026-09-28 21:31:59'),(2,'Banrural','9876543210','MONETARIA','SIGEMIC','ACTIVO','2026-09-28 21:31:59');
/*!40000 ALTER TABLE `bancos` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `clientes`
--

LOCK TABLES `clientes` WRITE;
/*!40000 ALTER TABLE `clientes` DISABLE KEYS */;
INSERT INTO `clientes` VALUES (1,2,'CLI-000002',_binary '3210884411601',_binary '\’B\’xÑ¨\◊\ﬁ˜Mm¶\0π\"±d\n\◊W˛\◊2\ﬂY¸\◊\‘\÷','2000-12-18',_binary '17 Calle 3-08 Zona 10',_binary '59014960','Ingeniero en Sistemas','GEGSA',15000.00,'2026-08-17 06:41:31','ACTIVO'),(2,3,'CLI-000003',_binary '3610556311601',_binary 'w∞DFzm&Öù1eB]ŒπØ\Èˆ-∞\œ\ÁaΩ6|\nb%\\','1997-08-02',_binary '1ra avenia 5-12 zona 1',_binary '40367054','Secretaria oficinista','Grupo Empresarial Garcia',4000.00,'2026-08-19 12:41:57','ACTIVO'),(3,5,'CLI-000005',_binary '3255080111601',_binary 'N\Œd\Ô∞S\⁄À§\≈C\√úê\Œjá¿\ÕŒ©]´\≈\„~ád','2000-08-31',_binary '1ra avenia 5-12 zona 1',_binary '58123529','Perito Contador','SIT',4000.00,'2026-08-26 23:21:14','ACTIVO'),(4,8,'CLI-000008',_binary '1234567952112',_binary '©sç≤\‹\‚Y\Ó	RIW¨/º•ùw∞g˙âîÖ\‰@Gö','1999-06-30',_binary '1ra avenia 5-12 zona 2',_binary '56986336','Perito Contador','GTA',15000.00,'2026-10-26 22:22:55','ACTIVO');
/*!40000 ALTER TABLE `clientes` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `creditos`
--

LOCK TABLES `creditos` WRITE;
/*!40000 ALTER TABLE `creditos` DISABLE KEYS */;
INSERT INTO `creditos` VALUES (1,'CR-000001',2,1,10000.00,'ANUAL',18.0000,12,916.80,10000.00,'ACTIVO','2026-08-26','2026-08-26','2027-08-26',NULL),(2,'CR-000002',1,1,5000.00,'ANUAL',18.0000,6,877.63,4197.37,'ACTIVO','2026-08-26','2026-08-26','2027-02-26',NULL),(3,'CR-000003',3,1,10000.00,'ANUAL',18.0000,12,916.80,0.00,'CANCELADO','2026-08-26','2026-08-26','2027-05-26','2026-10-26'),(4,'CR-000004',4,1,25000.00,'ANUAL',18.0000,36,903.81,23389.66,'ACTIVO','2026-10-26','2026-10-26','2029-10-26',NULL);
/*!40000 ALTER TABLE `creditos` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_creditos_validar_insert` BEFORE INSERT ON `creditos` FOR EACH ROW BEGIN

    DECLARE v_estado_cliente VARCHAR(20);
    DECLARE v_estado_plan VARCHAR(20);

    -- =====================================================
    -- 1. VALIDAR MONTO DEL CR√âDITO
    -- =====================================================

    IF NEW.monto_original <= 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El monto del cr√©dito debe ser mayor que cero';

    END IF;


    -- =====================================================
    -- 2. VALIDAR CUOTA
    -- =====================================================

    IF NEW.cuota_original <= 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La cuota del cr√©dito debe ser mayor que cero';

    END IF;


    -- =====================================================
    -- 3. VALIDAR SALDO DE CAPITAL
    -- =====================================================

    IF NEW.saldo_capital < 0
       OR NEW.saldo_capital > NEW.monto_original THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El saldo de capital no es v√°lido';

    END IF;


    -- =====================================================
    -- 4. VALIDAR PLAZO PERMITIDO
    -- =====================================================

    IF NEW.plazo_meses NOT IN
       (6, 12, 18, 24, 36, 48, 60) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El plazo del cr√©dito no est√° permitido';

    END IF;


    -- =====================================================
    -- 5. VALIDAR PORCENTAJE DE INTER√âS
    -- =====================================================

    IF NEW.porcentaje_interes_aplicado <= 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El porcentaje de inter√©s debe ser mayor que cero';

    END IF;


    -- =====================================================
    -- 6. VALIDAR FECHAS
    -- =====================================================

    IF NEW.fecha_inicio < NEW.fecha_otorgamiento THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La fecha de inicio no puede ser anterior al otorgamiento';

    END IF;


    IF NEW.fecha_finalizacion_estimada <= NEW.fecha_inicio THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La fecha de finalizaci√≥n debe ser posterior a la fecha de inicio';

    END IF;


    -- =====================================================
    -- 7. VALIDAR CLIENTE ACTIVO
    -- =====================================================

    SET v_estado_cliente =
    (
        SELECT estado
        FROM clientes
        WHERE id_cliente = NEW.id_cliente
        LIMIT 1
    );

    IF v_estado_cliente IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El cliente indicado no existe';

    END IF;


    IF v_estado_cliente <> 'ACTIVO' THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El cliente debe estar activo para recibir un cr√©dito';

    END IF;


    -- =====================================================
    -- 8. VALIDAR PLAN ACTIVO
    -- =====================================================

    SET v_estado_plan =
    (
        SELECT estado
        FROM planes_credito
        WHERE id_plan = NEW.id_plan
        LIMIT 1
    );

    IF v_estado_plan IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El plan de cr√©dito indicado no existe';

    END IF;


    IF v_estado_plan <> 'ACTIVO' THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El plan de cr√©dito debe estar activo';

    END IF;

END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

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
-- Dumping data for table `cuotas`
--

LOCK TABLES `cuotas` WRITE;
/*!40000 ALTER TABLE `cuotas` DISABLE KEYS */;
INSERT INTO `cuotas` VALUES (1,2,1,'2026-09-26',802.63,75.00,877.63,802.63,75.00,0.00,0.00,'PAGADA'),(2,2,2,'2026-10-26',814.67,62.96,877.63,0.00,0.00,0.00,877.63,'PENDIENTE'),(3,2,3,'2026-11-26',826.89,50.74,877.63,0.00,0.00,0.00,877.63,'PENDIENTE'),(4,2,4,'2026-12-26',839.29,38.34,877.63,0.00,0.00,0.00,877.63,'PENDIENTE'),(5,2,5,'2027-01-26',851.88,25.75,877.63,0.00,0.00,0.00,877.63,'PENDIENTE'),(6,2,6,'2027-02-26',864.64,12.97,877.61,0.00,0.00,0.00,877.61,'PENDIENTE'),(7,3,1,'2026-09-26',766.80,150.00,916.80,766.80,150.00,0.00,0.00,'PAGADA'),(8,3,2,'2026-10-26',778.30,138.50,916.80,778.30,138.50,0.00,0.00,'PAGADA'),(9,3,3,'2026-11-26',789.98,126.82,916.80,789.98,126.82,0.00,0.00,'PAGADA'),(10,3,4,'2026-12-26',831.83,84.97,916.80,831.83,84.97,0.00,0.00,'PAGADA'),(11,3,5,'2027-01-26',844.30,72.50,916.80,844.30,72.50,0.00,0.00,'PAGADA'),(12,3,6,'2027-02-26',856.97,59.83,916.80,856.97,59.83,0.00,0.00,'PAGADA'),(13,3,7,'2027-03-26',877.32,39.48,916.80,877.32,39.48,0.00,0.00,'PAGADA'),(14,3,8,'2027-04-26',890.48,26.32,916.80,890.48,26.32,0.00,0.00,'PAGADA'),(15,3,9,'2027-05-26',364.02,5.46,369.48,364.02,5.46,0.00,0.00,'PAGADA'),(19,4,1,'2026-11-26',528.81,375.00,903.81,528.81,375.00,0.00,0.00,'PAGADA'),(20,4,2,'2026-12-26',536.74,367.07,903.81,536.74,367.07,0.00,0.00,'PAGADA'),(21,4,3,'2027-01-26',544.79,359.02,903.81,544.79,359.02,0.00,0.00,'PAGADA'),(22,4,4,'2027-02-26',552.97,350.84,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(23,4,5,'2027-03-26',561.26,342.55,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(24,4,6,'2027-04-26',569.68,334.13,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(25,4,7,'2027-05-26',578.22,325.59,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(26,4,8,'2027-06-26',586.90,316.91,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(27,4,9,'2027-07-26',595.70,308.11,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(28,4,10,'2027-08-26',604.64,299.17,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(29,4,11,'2027-09-26',613.71,290.10,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(30,4,12,'2027-10-26',622.91,280.90,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(31,4,13,'2027-11-26',632.25,271.56,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(32,4,14,'2027-12-26',641.74,262.07,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(33,4,15,'2028-01-26',651.36,252.45,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(34,4,16,'2028-02-26',661.14,242.67,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(35,4,17,'2028-03-26',671.05,232.76,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(36,4,18,'2028-04-26',681.12,222.69,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(37,4,19,'2028-05-26',691.33,212.48,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(38,4,20,'2028-06-26',701.70,202.11,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(39,4,21,'2028-07-26',712.23,191.58,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(40,4,22,'2028-08-26',722.91,180.90,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(41,4,23,'2028-09-26',733.76,170.05,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(42,4,24,'2028-10-26',744.76,159.05,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(43,4,25,'2028-11-26',755.94,147.87,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(44,4,26,'2028-12-26',767.27,136.54,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(45,4,27,'2029-01-26',778.78,125.03,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(46,4,28,'2029-02-26',790.47,113.34,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(47,4,29,'2029-03-26',802.32,101.49,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(48,4,30,'2029-04-26',814.36,89.45,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(49,4,31,'2029-05-26',826.57,77.24,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(50,4,32,'2029-06-26',838.97,64.84,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(51,4,33,'2029-07-26',851.56,52.25,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(52,4,34,'2029-08-26',864.33,39.48,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(53,4,35,'2029-09-26',877.29,26.52,903.81,0.00,0.00,0.00,903.81,'PENDIENTE'),(54,4,36,'2029-10-26',890.46,13.36,903.82,0.00,0.00,0.00,903.82,'PENDIENTE');
/*!40000 ALTER TABLE `cuotas` ENABLE KEYS */;
UNLOCK TABLES;

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
-- Dumping data for table `documentos_cliente`
--

LOCK TABLES `documentos_cliente` WRITE;
/*!40000 ALTER TABLE `documentos_cliente` DISABLE KEYS */;
/*!40000 ALTER TABLE `documentos_cliente` ENABLE KEYS */;
UNLOCK TABLES;

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
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `formas_pago`
--

LOCK TABLES `formas_pago` WRITE;
/*!40000 ALTER TABLE `formas_pago` DISABLE KEYS */;
INSERT INTO `formas_pago` VALUES (1,'EFECTIVO','Pago realizado en efectivo','ACTIVO'),(2,'TRANSFERENCIA','Pago realizado mediante transferencia bancaria','ACTIVO'),(3,'DEPOSITO','Pago realizado mediante dep√≥sito bancario','ACTIVO');
/*!40000 ALTER TABLE `formas_pago` ENABLE KEYS */;
UNLOCK TABLES;

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
) ENGINE=InnoDB AUTO_INCREMENT=22 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pagos`
--

LOCK TABLES `pagos` WRITE;
/*!40000 ALTER TABLE `pagos` DISABLE KEYS */;
INSERT INTO `pagos` VALUES (1,3,7,4,1,NULL,NULL,NULL,'REC-000001','PAGO_CUOTA',916.80,766.80,150.00,0.00,'2026-08-26 23:35:27',NULL),(2,3,8,4,1,NULL,NULL,NULL,'REC-000002','PAGO_CUOTA',916.80,778.30,138.50,0.00,'2026-08-26 23:45:02',NULL),(3,3,9,4,1,NULL,NULL,NULL,'REC-000003','PAGO_CUOTA',400.00,273.18,126.82,0.00,'2026-08-26 23:53:37',NULL),(4,3,9,4,1,NULL,NULL,NULL,'REC-000004','PAGO_CUOTA',516.80,516.80,0.00,0.00,'2026-08-26 23:54:10',NULL),(5,3,NULL,4,1,NULL,NULL,NULL,'REC-000005','ABONO_EXTRAORDINARIO',2000.00,2000.00,0.00,0.00,'2026-08-27 00:03:59',NULL),(6,3,10,1,3,NULL,NULL,NULL,'REC-000006','PAGO_CUOTA',916.80,831.83,84.97,0.00,'2026-09-28 22:27:04','Pago aprobado desde solicitud #1. Banco: 1. Boleta: 40893717.'),(7,3,11,1,3,NULL,NULL,NULL,'REC-000007','PAGO_CUOTA',100.00,27.50,72.50,0.00,'2026-09-29 00:10:42',NULL),(8,3,11,1,1,NULL,NULL,NULL,'REC-000008','PAGO_CUOTA',816.80,816.80,0.00,0.00,'2026-09-29 00:24:58',NULL),(9,3,12,1,1,NULL,NULL,NULL,'REC-000009','PAGO_CUOTA',500.00,440.17,59.83,0.00,'2026-09-29 00:28:35',NULL),(10,3,12,1,1,NULL,NULL,NULL,'REC-000010','PAGO_CUOTA',416.80,416.80,0.00,0.00,'2026-09-29 00:30:32',NULL),(11,3,NULL,1,1,NULL,NULL,NULL,'REC-000011','ABONO_EXTRAORDINARIO',500.00,500.00,0.00,0.00,'2026-09-29 00:30:54',NULL),(12,3,13,1,3,NULL,NULL,NULL,'REC-000012','PAGO_CUOTA',100.00,60.52,39.48,0.00,'2026-09-29 00:37:50','Pago aprobado desde solicitud #2. Banco: Banco Industrial. Boleta: 59014960.'),(13,3,13,1,3,NULL,NULL,NULL,'REC-000013','PAGO_CUOTA',816.80,816.80,0.00,0.00,'2026-09-30 06:50:03','Pago aprobado desde solicitud #3. Banco: Banco Industrial. Boleta: 56545654.'),(14,4,19,1,3,NULL,NULL,NULL,'REC-000014','PAGO_CUOTA',903.81,528.81,375.00,0.00,'2026-10-26 22:46:09','pago'),(15,3,14,4,1,NULL,NULL,NULL,'REC-000015','PAGO_CUOTA',916.80,890.48,26.32,0.00,'2026-10-26 22:48:30','cuota'),(16,3,NULL,4,1,NULL,NULL,NULL,'REC-000016','ABONO_EXTRAORDINARIO',500.00,500.00,0.00,0.00,'2026-10-26 22:49:04','abono'),(17,4,20,4,1,NULL,NULL,NULL,'REC-000017','PAGO_CUOTA',903.81,536.74,367.07,0.00,'2026-10-26 23:02:44','pago'),(18,3,15,4,1,NULL,NULL,NULL,'REC-000018','PAGO_CUOTA',369.48,364.02,5.46,0.00,'2026-10-26 23:03:04','pago'),(19,2,1,4,3,1,'123456','pago_99d019cf7dab4082ba7b81d89f003948.jpeg','REC-000019','PAGO_CUOTA',877.63,802.63,75.00,0.00,'2026-10-26 23:49:56','pago'),(20,4,21,1,3,2,'1234567','68a9f92d90714ff19c969ca3bf346abd.jpeg','REC-000020','PAGO_CUOTA',500.00,140.98,359.02,0.00,'2026-11-01 20:23:15','Pago aprobado desde solicitud #4. Banco: Banrural. Boleta: 1234567.'),(21,4,21,1,3,1,'123ABC','39488dbeacf54207ba319981938efc60.jpeg','REC-000021','PAGO_CUOTA',403.81,403.81,0.00,0.00,'2026-11-02 01:57:57','Pago aprobado desde solicitud #5. Banco: Banco Industrial. Boleta: 123ABC.');
/*!40000 ALTER TABLE `pagos` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_pagos_validar_insert` BEFORE INSERT ON `pagos` FOR EACH ROW BEGIN

    DECLARE v_forma_pago VARCHAR(50);

    -- =====================================================
    -- 1. VALIDAR MONTO TOTAL
    -- =====================================================

    IF NEW.monto_total <= 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'El monto del pago debe ser mayor que cero';

    END IF;


    -- =====================================================
    -- 2. VALIDAR VALORES NEGATIVOS
    -- =====================================================

    IF NEW.capital_aplicado < 0
       OR NEW.interes_aplicado < 0
       OR NEW.mora_aplicada < 0 THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Capital, inter√©s y mora no pueden ser negativos';

    END IF;


    -- =====================================================
    -- 3. VALIDAR DISTRIBUCI√ìN DEL PAGO
    -- capital + inter√©s + mora = monto total
    -- =====================================================

    IF ROUND(
        NEW.capital_aplicado +
        NEW.interes_aplicado +
        NEW.mora_aplicada,
        2
    ) <> ROUND(NEW.monto_total, 2) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La distribuci√≥n del pago no coincide con el monto total';

    END IF;


    -- =====================================================
    -- 4. PAGO DE CUOTA DEBE TENER CUOTA
    -- =====================================================

    IF NEW.tipo_operacion = 'PAGO_CUOTA'
       AND NEW.id_cuota IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Un pago de cuota debe tener una cuota asociada';

    END IF;


    -- =====================================================
    -- 5. ABONO EXTRAORDINARIO NO DEBE ESTAR
    --    ASOCIADO A UNA CUOTA ESPEC√çFICA
    -- =====================================================

    IF NEW.tipo_operacion = 'ABONO_EXTRAORDINARIO'
       AND NEW.id_cuota IS NOT NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Un abono extraordinario no debe tener una cuota asociada';

    END IF;


    -- =====================================================
    -- 6. VALIDAR FORMA DE PAGO ACTIVA
    -- =====================================================

    SET v_forma_pago =
    (
        SELECT nombre
        FROM formas_pago
        WHERE
            id_forma_pago = NEW.id_forma_pago
            AND estado = 'ACTIVO'
        LIMIT 1
    );


    IF v_forma_pago IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La forma de pago indicada no existe o est√° inactiva';

    END IF;


    -- =====================================================
    -- 7. SI ES EFECTIVO NO DEBE TENER DATOS BANCARIOS
    -- =====================================================

    IF v_forma_pago = 'EFECTIVO'
       AND (
            NEW.id_banco IS NOT NULL
            OR NEW.numero_boleta IS NOT NULL
       ) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Un pago en efectivo no debe contener informaci√≥n bancaria';

    END IF;


    -- =====================================================
    -- 8. TRANSFERENCIA O DEP√ìSITO REQUIEREN BANCO Y N√öMERO DE BOLETA
    -- =====================================================

    IF v_forma_pago IN ('TRANSFERENCIA', 'DEPOSITO')
       AND (
            NEW.id_banco IS NULL
            OR NEW.numero_boleta IS NULL
            OR TRIM(NEW.numero_boleta) = ''
       ) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'Los pagos bancarios requieren banco y n√∫mero de boleta';

    END IF;

END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

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
) ENGINE=InnoDB AUTO_INCREMENT=40 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `permisos`
--

LOCK TABLES `permisos` WRITE;
/*!40000 ALTER TABLE `permisos` DISABLE KEYS */;
INSERT INTO `permisos` VALUES (1,'CLIENTES_VER','Ver clientes','Permite consultar el listado y datos generales de clientes','CLIENTES','ACTIVO','2026-10-16 22:03:24'),(2,'CLIENTES_CREAR','Crear clientes','Permite registrar nuevos clientes','CLIENTES','ACTIVO','2026-10-16 22:03:24'),(3,'CLIENTES_EDITAR','Editar clientes','Permite modificar informaci√≥n de clientes','CLIENTES','ACTIVO','2026-10-16 22:03:24'),(4,'CLIENTES_CAMBIAR_ESTADO','Cambiar estado de clientes','Permite activar o inactivar clientes','CLIENTES','ACTIVO','2026-10-16 22:03:24'),(5,'CREDITOS_VER','Ver cr√©ditos','Permite consultar cr√©ditos registrados','CREDITOS','ACTIVO','2026-10-16 22:03:24'),(6,'CREDITOS_CREAR','Otorgar cr√©ditos','Permite crear nuevos cr√©ditos para clientes','CREDITOS','ACTIVO','2026-10-16 22:03:24'),(7,'CREDITOS_DETALLE','Ver detalle de cr√©ditos','Permite consultar cuotas, saldo y detalle de un cr√©dito','CREDITOS','ACTIVO','2026-10-16 22:03:24'),(8,'PAGOS_REGISTRAR','Registrar pagos','Permite registrar pagos de cuotas','PAGOS','ACTIVO','2026-10-16 22:03:24'),(9,'PAGOS_ABONO_CAPITAL','Registrar abonos a capital','Permite realizar abonos extraordinarios a capital','PAGOS','ACTIVO','2026-10-16 22:03:24'),(10,'PAGOS_HISTORIAL','Ver historial de pagos','Permite consultar el historial de pagos realizados','PAGOS','ACTIVO','2026-10-16 22:03:24'),(11,'SOLICITUDES_VER','Ver solicitudes de pago','Permite consultar solicitudes enviadas por clientes','SOLICITUDES','ACTIVO','2026-10-16 22:03:24'),(12,'SOLICITUDES_APROBAR','Aprobar solicitudes','Permite aprobar solicitudes de pago','SOLICITUDES','ACTIVO','2026-10-16 22:03:24'),(13,'SOLICITUDES_RECHAZAR','Rechazar solicitudes','Permite rechazar solicitudes de pago','SOLICITUDES','ACTIVO','2026-10-16 22:03:24');
/*!40000 ALTER TABLE `permisos` ENABLE KEYS */;
UNLOCK TABLES;

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
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `planes_credito`
--

LOCK TABLES `planes_credito` WRITE;
/*!40000 ALTER TABLE `planes_credito` DISABLE KEYS */;
INSERT INTO `planes_credito` VALUES (1,'Microcr√©dito Personal Anual','Plan inicial de ejemplo con tasa anual del 18%','ANUAL',18.0000,'ACTIVO','2026-08-12 22:10:25'),(2,'Microcr√©dito Personal Mensual','Plan de ejemplo con tasa mensual','MENSUAL',2.5000,'ACTIVO','2026-08-12 22:10:25'),(3,'Microcr√©dito Emprendedor','Cr√©dito destinado a peque√±os negocios','ANUAL',20.0000,'ACTIVO','2026-08-26 22:38:43'),(4,'Plan Anual 18%','Plan de cr√©dito con tasa nominal anual del 18%','ANUAL',18.0000,'ACTIVO','2026-11-01 21:35:59'),(5,'Plan Mensual 1.5%','Plan de cr√©dito con tasa mensual del 1.5%','MENSUAL',1.5000,'ACTIVO','2026-11-01 21:35:59');
/*!40000 ALTER TABLE `planes_credito` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `recuperaciones_password`
--

DROP TABLE IF EXISTS `recuperaciones_password`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `recuperaciones_password` (
  `id_recuperacion` bigint unsigned NOT NULL AUTO_INCREMENT,
  `id_usuario` int unsigned NOT NULL,
  `token_hash` binary(32) NOT NULL,
  `fecha_solicitud` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `fecha_expiracion` datetime NOT NULL,
  `fecha_utilizacion` datetime DEFAULT NULL,
  `utilizado` tinyint(1) NOT NULL DEFAULT '0',
  `direccion_ip` varchar(45) DEFAULT NULL,
  PRIMARY KEY (`id_recuperacion`),
  UNIQUE KEY `uq_recuperaciones_password_token` (`token_hash`),
  KEY `idx_recuperaciones_usuario` (`id_usuario`),
  KEY `idx_recuperaciones_expiracion` (`fecha_expiracion`),
  CONSTRAINT `fk_recuperaciones_password_usuario` FOREIGN KEY (`id_usuario`) REFERENCES `usuarios` (`id_usuario`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `recuperaciones_password`
--

LOCK TABLES `recuperaciones_password` WRITE;
/*!40000 ALTER TABLE `recuperaciones_password` DISABLE KEYS */;
INSERT INTO `recuperaciones_password` VALUES (1,2,_binary 'Æck6w\‚†O†êÒ6\"Úy3®DÆd\ÕoâE	/°L\Íê','2026-11-01 20:47:46','2026-11-01 21:17:46','2026-11-01 21:11:48',1,'::1'),(2,2,_binary '\›P\ÍF∑´\Ÿ¡¢\÷\'<ç «Å^ú	û~H∑y\÷Õ∏¸\Ì','2026-11-01 21:11:48','2026-11-01 21:41:48','2026-11-01 21:13:45',1,'::1');
/*!40000 ALTER TABLE `recuperaciones_password` ENABLE KEYS */;
UNLOCK TABLES;

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
  UNIQUE KEY `uq_solicitudes_pago_banco_boleta` (`id_banco`,`numero_boleta`),
  UNIQUE KEY `uq_solicitud_pago` (`id_pago`),
  KEY `fk_solicitud_cliente` (`id_cliente`),
  KEY `fk_solicitud_credito` (`id_credito`),
  KEY `fk_solicitud_cuota` (`id_cuota`),
  KEY `fk_solicitud_usuario_revision` (`id_usuario_revision`),
  CONSTRAINT `fk_solicitud_banco` FOREIGN KEY (`id_banco`) REFERENCES `bancos` (`id_banco`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_cliente` FOREIGN KEY (`id_cliente`) REFERENCES `clientes` (`id_cliente`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_credito` FOREIGN KEY (`id_credito`) REFERENCES `creditos` (`id_credito`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_cuota` FOREIGN KEY (`id_cuota`) REFERENCES `cuotas` (`id_cuota`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_pago` FOREIGN KEY (`id_pago`) REFERENCES `pagos` (`id_pago`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_solicitud_usuario_revision` FOREIGN KEY (`id_usuario_revision`) REFERENCES `usuarios` (`id_usuario`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `chk_solicitud_monto` CHECK ((`monto` > 0))
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `solicitudes_pago`
--

LOCK TABLES `solicitudes_pago` WRITE;
/*!40000 ALTER TABLE `solicitudes_pago` DISABLE KEYS */;
INSERT INTO `solicitudes_pago` VALUES (1,3,3,10,1,1,6,'PAGO_CUOTA',916.80,'40893717','48ac99ab98664d83ab03f18e94f787dc.jpeg','APROBADA','2026-09-28 22:14:09','2026-09-28 22:27:05',NULL),(2,3,3,13,1,1,12,'PAGO_CUOTA',100.00,'59014960','06360e8b6d644651b69ee41c6d598ba8.jpg','APROBADA','2026-09-29 00:37:13','2026-09-29 00:37:50',NULL),(3,3,3,13,1,1,13,'PAGO_CUOTA',816.80,'56545654','666d15d90c6c4e708ce4b51aa243df20.png','APROBADA','2026-09-30 06:49:17','2026-09-30 06:50:03',NULL),(4,4,4,21,2,1,20,'PAGO_CUOTA',500.00,'1234567','68a9f92d90714ff19c969ca3bf346abd.jpeg','APROBADA','2026-10-26 23:56:41','2026-11-01 20:23:15',NULL),(5,4,4,21,1,1,21,'PAGO_CUOTA',403.81,'123ABC','39488dbeacf54207ba319981938efc60.jpeg','APROBADA','2026-11-02 01:45:44','2026-11-02 01:57:58',NULL);
/*!40000 ALTER TABLE `solicitudes_pago` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_solicitudes_pago_validar_update` BEFORE UPDATE ON `solicitudes_pago` FOR EACH ROW BEGIN

    -- =====================================================
    -- 1. NO PERMITIR CAMBIAR EL ESTADO DE UNA SOLICITUD QUE YA FUE REVISADA
    -- =====================================================

    IF OLD.estado IN ('APROBADA', 'RECHAZADA')
       AND NEW.estado <> OLD.estado THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT =
            'La solicitud ya fue revisada y su estado no puede modificarse';

    END IF;


    -- =====================================================
    -- 2. VALIDACIONES PARA APROBAR
    -- =====================================================

    IF NEW.estado = 'APROBADA' THEN

        IF NEW.id_pago IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud aprobada debe tener un pago asociado';

        END IF;


        IF NEW.id_usuario_revision IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud aprobada debe indicar el usuario revisor';

        END IF;


        IF NEW.fecha_revision IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud aprobada debe registrar fecha de revisi√≥n';

        END IF;

    END IF;


    -- =====================================================
    -- 3. VALIDACIONES PARA RECHAZAR
    -- =====================================================

    IF NEW.estado = 'RECHAZADA' THEN

        IF NEW.id_pago IS NOT NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud rechazada no puede tener un pago asociado';

        END IF;


        IF NEW.id_usuario_revision IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud rechazada debe indicar el usuario revisor';

        END IF;


        IF NEW.fecha_revision IS NULL THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud rechazada debe registrar fecha de revisi√≥n';

        END IF;


        IF NEW.motivo_rechazo IS NULL
           OR TRIM(NEW.motivo_rechazo) = '' THEN

            SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT =
                'Una solicitud rechazada debe indicar el motivo del rechazo';

        END IF;

    END IF;

END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

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
-- Dumping data for table `usuario_permisos`
--

LOCK TABLES `usuario_permisos` WRITE;
/*!40000 ALTER TABLE `usuario_permisos` DISABLE KEYS */;
INSERT INTO `usuario_permisos` VALUES (4,1,'2026-11-02 00:25:01'),(4,2,'2026-11-02 00:25:01'),(4,3,'2026-11-02 00:25:01'),(4,4,'2026-11-02 00:25:01'),(4,5,'2026-11-02 00:25:01'),(4,6,'2026-11-02 00:25:01'),(4,7,'2026-11-02 00:25:01'),(4,8,'2026-11-02 00:25:01'),(4,9,'2026-11-02 00:25:01'),(4,10,'2026-11-02 00:25:01'),(6,1,'2026-10-16 23:55:53'),(6,2,'2026-10-16 23:55:53'),(6,5,'2026-10-16 23:55:53'),(6,7,'2026-10-16 23:55:53'),(6,8,'2026-10-16 23:55:53'),(6,10,'2026-10-16 23:55:53'),(6,11,'2026-10-16 23:55:53'),(6,12,'2026-10-16 23:55:53'),(7,1,'2026-10-26 22:18:24'),(7,2,'2026-10-26 22:18:24'),(7,3,'2026-10-26 22:18:24'),(7,4,'2026-10-26 22:18:24'),(7,5,'2026-10-26 22:18:24'),(7,6,'2026-10-26 22:18:24'),(7,7,'2026-10-26 22:18:24'),(7,8,'2026-10-26 22:18:24'),(7,9,'2026-10-26 22:18:24'),(7,10,'2026-10-26 22:18:24'),(7,11,'2026-10-26 22:18:24'),(7,12,'2026-10-26 22:18:24'),(7,13,'2026-10-26 22:18:24');
/*!40000 ALTER TABLE `usuario_permisos` ENABLE KEYS */;
UNLOCK TABLES;

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
  `correo_hash` binary(32) DEFAULT NULL,
  `telefono_cifrado` varbinary(512) DEFAULT NULL,
  `rol` enum('ADMINISTRADOR','CAJERO','CLIENTE') NOT NULL,
  `estado` enum('ACTIVO','INACTIVO','BLOQUEADO') NOT NULL DEFAULT 'ACTIVO',
  `intentos_fallidos` tinyint unsigned NOT NULL DEFAULT '0',
  `ultimo_acceso` datetime DEFAULT NULL,
  `fecha_creacion` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`id_usuario`),
  UNIQUE KEY `uq_usuarios_username` (`username`),
  UNIQUE KEY `uq_usuarios_correo_hash` (`correo_hash`)
) ENGINE=InnoDB AUTO_INCREMENT=10 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'Administrador','SIGEMIC','admin','AQAAAAIAAYagAAAAEIvma+z4PN3t73oBsPwizD/Y/Hp5FQu6o6gOpUWZ3LPIaBL7YhTfQCTv6E7FMO1KvQ==',_binary 'josecu132@gmail.com',_binary 'ªÖGou`&rï.6[	®N!\“¯ìB)q}\√;\‡s˛•9\Í',NULL,'ADMINISTRADOR','ACTIVO',0,'2026-11-02 01:56:50','2026-08-13 00:57:36'),(2,'David','Cu Tot','Davidtot','AQAAAAIAAYagAAAAEKUJafE4RsAEDCtgNF4Ckn4DDLvUKUwSjQ90tITRKAhBFgaB5JvBl+NeWWlJWd0cdA==',_binary 'davidtot145@gmail.com',_binary ')\Ê/\'\'r$p[YñqÚ^L\‹FπYÑ\Z\⁄k\“\‰8S)f',NULL,'CLIENTE','ACTIVO',0,'2026-11-01 23:42:44','2026-08-17 06:41:31'),(3,'Alma','Tot','Almatot','AQAAAAIAAYagAAAAEG49F7wlfCy2DwYmYYDU2m4kLwgPXdAw3d19Lcr0PCG5iAfEgJ1Q2BA9iQtsk31zKw==',NULL,NULL,NULL,'CLIENTE','ACTIVO',0,NULL,'2026-08-19 12:41:57'),(4,'Cajero','01','cajero01','AQAAAAIAAYagAAAAEPN3eoQlbSLucJSMoLJAoiOMFi65D/RijmdFKBMH73jtGIf953bQ+LtryZ1O3TnLeA==',_binary 'cajero01@gmail.com',NULL,NULL,'CAJERO','ACTIVO',0,'2026-11-02 00:25:07','2026-08-19 12:50:40'),(5,'Evelyn Raquel','Laj','Evelynlaj','AQAAAAIAAYagAAAAEDdX9g7HOAX6QOUQDk0cLyalli7E5x6gaxrFVj2seLp242ZQfkTFMCPfXzCFZHXKZg==',NULL,NULL,NULL,'CLIENTE','ACTIVO',0,NULL,'2026-08-26 23:21:14'),(6,'Victor','Laj','Victor','AQAAAAIAAYagAAAAEA8Q6LQ7T7SL+UACInfQY5iPfyms9kBPeb1qv4X6MjXxJkUmX1dTP9d4zzU/W85bww==',_binary 'victor@gmail.com',NULL,NULL,'CAJERO','ACTIVO',0,'2026-11-02 01:44:30','2026-10-16 22:34:25'),(7,'Daniel','Mendez','Danielm','AQAAAAIAAYagAAAAEIKA+4daW+uM4YaX1vUllWqLE/bYDAIEozeALu+qEES5JezV0d+dtiXkEwywLh1EoQ==',_binary 'danielmendez@gmail.com',NULL,NULL,'CAJERO','ACTIVO',0,NULL,'2026-10-26 22:18:24'),(8,'Estela Elizabeth','Cu','estelac','AQAAAAIAAYagAAAAEDGf/E0/HO8zRI0yGiZxjzIqMXnFWZ7QOL+2P9jFMgyFIkYRDes7QD/UiPBGmrHYgA==',NULL,NULL,NULL,'CLIENTE','ACTIVO',0,'2026-11-02 01:44:46','2026-10-26 22:22:55'),(9,'Jose','Tot','josetot','AQAAAAIAAYagAAAAEB/MAjlwucgn+iDfA0+Vp9usPzqf5G1u5GyxAnfz3SZpst9abEvcM6FT1BVwySB5jQ==',_binary 'josecu@gmail.com',_binary 'ZÅâ\ƒ Q\–-Ñ+a\·}È´∫µœÜÆ±°ø+¯¶\¬\„',NULL,'ADMINISTRADOR','ACTIVO',0,'2026-10-28 03:15:31','2026-10-28 03:04:33');
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'sigemic'
--
/*!50003 DROP PROCEDURE IF EXISTS `sp_estado_cuenta_cliente` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_estado_cuenta_cliente`(
    IN p_id_cliente INT UNSIGNED
)
BEGIN

    -- Validar que el cliente exista
    IF NOT EXISTS (
        SELECT 1
        FROM clientes
        WHERE id_cliente = p_id_cliente
    ) THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El cliente indicado no existe';
    END IF;


    -- Generar estado de cuenta
    SELECT
        c.id_cliente AS 'ID Cliente',
        c.codigo_cliente AS 'C√≥digo cliente',

        CONCAT(
            u.nombres,
            ' ',
            u.apellidos
        ) AS 'Cliente',

        c.estado AS 'Estado cliente',

        (
            SELECT COUNT(*)
            FROM creditos cr
            WHERE cr.id_cliente = c.id_cliente
        ) AS 'Total cr√©ditos',

        COALESCE(
            (
                SELECT SUM(cr.monto_original)
                FROM creditos cr
                WHERE cr.id_cliente = c.id_cliente
            ),
            0
        ) AS 'Total otorgado',

        COALESCE(
            (
                SELECT SUM(cr.saldo_capital)
                FROM creditos cr
                WHERE cr.id_cliente = c.id_cliente
            ),
            0
        ) AS 'Saldo capital',

        COALESCE(
            (
                SELECT SUM(p.monto_total)
                FROM pagos p
                INNER JOIN creditos cr
                    ON cr.id_credito = p.id_credito
                WHERE cr.id_cliente = c.id_cliente
            ),
            0
        ) AS 'Total pagado',

        (
            SELECT COUNT(*)
            FROM cuotas cu
            INNER JOIN creditos cr
                ON cr.id_credito = cu.id_credito
            WHERE
                cr.id_cliente = c.id_cliente
                AND cu.estado = 'PAGADA'
        ) AS 'Cuotas pagadas',

        (
            SELECT COUNT(*)
            FROM cuotas cu
            INNER JOIN creditos cr
                ON cr.id_credito = cu.id_credito
            WHERE
                cr.id_cliente = c.id_cliente
                AND cu.estado IN (
                    'PENDIENTE',
                    'PARCIAL',
                    'ATRASADA'
                )
        ) AS 'Cuotas pendientes',

        (
            SELECT MIN(cu.fecha_vencimiento)
            FROM cuotas cu
            INNER JOIN creditos cr
                ON cr.id_credito = cu.id_credito
            WHERE
                cr.id_cliente = c.id_cliente
                AND cu.estado IN (
                    'PENDIENTE',
                    'PARCIAL',
                    'ATRASADA'
                )
        ) AS 'Pr√≥ximo vencimiento'

    FROM clientes c

    INNER JOIN usuarios u
        ON u.id_usuario = c.id_usuario

    WHERE c.id_cliente = p_id_cliente;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_pagos_por_periodo` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_pagos_por_periodo`(
    IN p_fecha_inicio DATE,
    IN p_fecha_fin DATE
)
BEGIN

    -- =====================================================
    -- VALIDAR FECHAS
    -- =====================================================

    IF p_fecha_inicio IS NULL OR p_fecha_fin IS NULL THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Las fechas de inicio y fin son obligatorias';

    END IF;


    IF p_fecha_inicio > p_fecha_fin THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'La fecha inicial no puede ser mayor que la fecha final';

    END IF;


    -- =====================================================
    -- RESULTADO 1
    -- RESUMEN GENERAL DEL PER√çODO
    -- =====================================================

    SELECT
        p_fecha_inicio AS 'Fecha inicial',

        p_fecha_fin AS 'Fecha final',

        COUNT(p.id_pago) AS 'Cantidad de pagos',

        COALESCE(
            SUM(p.monto_total),
            0
        ) AS 'Total recibido',

        COALESCE(
            SUM(p.capital_aplicado),
            0
        ) AS 'Capital aplicado',

        COALESCE(
            SUM(p.interes_aplicado),
            0
        ) AS 'Inter√©s aplicado',

        COALESCE(
            SUM(p.mora_aplicada),
            0
        ) AS 'Mora aplicada',

        COALESCE(
            AVG(p.monto_total),
            0
        ) AS 'Promedio por pago'

    FROM pagos p

    WHERE DATE(p.fecha_pago)
        BETWEEN p_fecha_inicio AND p_fecha_fin;


    -- =====================================================
    -- RESULTADO 2
    -- RESUMEN POR FORMA DE PAGO
    -- =====================================================

    SELECT
        fp.nombre AS 'Forma de pago',

        COUNT(p.id_pago) AS 'Cantidad',

        SUM(p.monto_total) AS 'Total recibido',

        SUM(p.capital_aplicado) AS 'Capital',

        SUM(p.interes_aplicado) AS 'Inter√©s',

        AVG(p.monto_total) AS 'Promedio'

    FROM pagos p

    INNER JOIN formas_pago fp
        ON fp.id_forma_pago = p.id_forma_pago

    WHERE DATE(p.fecha_pago)
        BETWEEN p_fecha_inicio AND p_fecha_fin

    GROUP BY
        fp.id_forma_pago,
        fp.nombre

    ORDER BY
        SUM(p.monto_total) DESC;


    -- =====================================================
    -- RESULTADO 3
    -- DETALLE DE PAGOS DEL PER√çODO
    -- =====================================================

    SELECT
        p.numero_recibo AS 'Recibo',

        DATE(p.fecha_pago) AS 'Fecha',

        cr.codigo_credito AS 'Cr√©dito',

        c.codigo_cliente AS 'C√≥digo cliente',

        CONCAT(
            u.nombres,
            ' ',
            u.apellidos
        ) AS 'Cliente',

        fp.nombre AS 'Forma de pago',

        p.tipo_operacion AS 'Operaci√≥n',

        p.monto_total AS 'Monto',

        p.capital_aplicado AS 'Capital',

        p.interes_aplicado AS 'Inter√©s',

        p.mora_aplicada AS 'Mora'

    FROM pagos p

    INNER JOIN creditos cr
        ON cr.id_credito = p.id_credito

    INNER JOIN clientes c
        ON c.id_cliente = cr.id_cliente

    INNER JOIN usuarios u
        ON u.id_usuario = c.id_usuario

    INNER JOIN formas_pago fp
        ON fp.id_forma_pago = p.id_forma_pago

    WHERE DATE(p.fecha_pago)
        BETWEEN p_fecha_inicio AND p_fecha_fin

    ORDER BY p.fecha_pago DESC;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
/*!50003 DROP PROCEDURE IF EXISTS `sp_resumen_credito` */;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
CREATE DEFINER=`root`@`localhost` PROCEDURE `sp_resumen_credito`(
    IN p_id_credito BIGINT UNSIGNED
)
BEGIN

    -- =====================================================
    -- VALIDAR QUE EL CR√âDITO EXISTA
    -- =====================================================

    IF NOT EXISTS (
        SELECT 1
        FROM creditos
        WHERE id_credito = p_id_credito
    ) THEN

        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'El cr√©dito indicado no existe';

    END IF;


    -- =====================================================
    -- RESULTADO 1
    -- RESUMEN FINANCIERO DEL CR√âDITO
    -- =====================================================

    SELECT
        cr.id_credito AS 'ID Cr√©dito',

        cr.codigo_credito AS 'C√≥digo cr√©dito',

        c.codigo_cliente AS 'C√≥digo cliente',

        CONCAT(
            u.nombres,
            ' ',
            u.apellidos
        ) AS 'Cliente',

        pc.nombre AS 'Plan',

        cr.monto_original AS 'Monto original',

        cr.tipo_tasa_aplicada AS 'Tipo de tasa',

        cr.porcentaje_interes_aplicado AS 'Inter√©s %',

        cr.plazo_meses AS 'Plazo meses',

        cr.cuota_original AS 'Cuota original',

        cr.saldo_capital AS 'Saldo capital',

        cr.estado AS 'Estado',

        cr.fecha_otorgamiento AS 'Fecha otorgamiento',

        cr.fecha_inicio AS 'Fecha inicio',

        cr.fecha_finalizacion_estimada AS 'Finalizaci√≥n estimada',

        COALESCE(
            (
                SELECT SUM(p.monto_total)
                FROM pagos p
                WHERE p.id_credito = cr.id_credito
            ),
            0
        ) AS 'Total pagado',

        COALESCE(
            (
                SELECT SUM(p.capital_aplicado)
                FROM pagos p
                WHERE p.id_credito = cr.id_credito
            ),
            0
        ) AS 'Capital pagado',

        COALESCE(
            (
                SELECT SUM(p.interes_aplicado)
                FROM pagos p
                WHERE p.id_credito = cr.id_credito
            ),
            0
        ) AS 'Inter√©s pagado',

        COALESCE(
            (
                SELECT SUM(p.mora_aplicada)
                FROM pagos p
                WHERE p.id_credito = cr.id_credito
            ),
            0
        ) AS 'Mora pagada',

        (
            SELECT COUNT(*)
            FROM cuotas cu
            WHERE
                cu.id_credito = cr.id_credito
                AND cu.estado = 'PAGADA'
        ) AS 'Cuotas pagadas',

        (
            SELECT COUNT(*)
            FROM cuotas cu
            WHERE
                cu.id_credito = cr.id_credito
                AND cu.estado IN (
                    'PENDIENTE',
                    'PARCIAL',
                    'ATRASADA'
                )
        ) AS 'Cuotas pendientes'

    FROM creditos cr

    INNER JOIN clientes c
        ON c.id_cliente = cr.id_cliente

    INNER JOIN usuarios u
        ON u.id_usuario = c.id_usuario

    INNER JOIN planes_credito pc
        ON pc.id_plan = cr.id_plan

    WHERE cr.id_credito = p_id_credito;


    -- =====================================================
    -- RESULTADO 2
    -- DETALLE DE CUOTAS / TABLA DE AMORTIZACI√ìN
    -- =====================================================

    SELECT
        cu.numero_cuota AS 'Cuota',

        cu.fecha_vencimiento AS 'Vencimiento',

        cu.capital_programado AS 'Capital programado',

        cu.interes_programado AS 'Inter√©s programado',

        cu.cuota_programada AS 'Cuota programada',

        cu.capital_pagado AS 'Capital pagado',

        cu.interes_pagado AS 'Inter√©s pagado',

        cu.mora_pagada AS 'Mora pagada',

        cu.saldo_cuota AS 'Saldo cuota',

        cu.estado AS 'Estado'

    FROM cuotas cu

    WHERE cu.id_credito = p_id_credito

    ORDER BY cu.numero_cuota;

END ;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;
SET @@SESSION.SQL_LOG_BIN = @MYSQLDUMP_TEMP_LOG_BIN;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-11-02  2:33:19
