/*
 Navicat Premium Data Transfer

 Target Server Type    : MySQL
 Target Server Version : 50739
 File Encoding         : 65001

 Date: 16/04/2023 13:35:34
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for rss_about
-- ----------------------------
DROP TABLE IF EXISTS `rss_about`;
CREATE TABLE `rss_about`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `text` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `is_pro` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for rss_category
-- ----------------------------
DROP TABLE IF EXISTS `rss_category`;
CREATE TABLE `rss_category`  (
  `id` smallint(6) NOT NULL AUTO_INCREMENT,
  `name` varchar(191) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `attributes` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `update_time` date NULL DEFAULT NULL,
  `create_time` date NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `name`(`name`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 7 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for rss_favorite_entry
-- ----------------------------
DROP TABLE IF EXISTS `rss_favorite_entry`;
CREATE TABLE `rss_favorite_entry`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `u_id` int(11) NULL DEFAULT NULL COMMENT '用户id',
  `fe_id` int(11) NULL DEFAULT NULL COMMENT '关联rss_feed_entry',
  `create_date` datetime NULL DEFAULT NULL,
  `update_date` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `pk_`(`u_id`, `fe_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 272 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for rss_feed_entry
-- ----------------------------
DROP TABLE IF EXISTS `rss_feed_entry`;
CREATE TABLE `rss_feed_entry`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `f_id` int(11) NOT NULL COMMENT '关联 rss_feeds ',
  `title` varchar(768) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `sub_title` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `author` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `content` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `link` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `lastseen` int(11) NULL DEFAULT NULL,
  `hash` longblob NULL,
  `publishingdate` datetime NULL DEFAULT NULL,
  `image_url` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `guid` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  `feed_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `pk_createdate`(`create_time`) USING BTREE,
  INDEX `pk_fid`(`f_id`) USING BTREE,
  INDEX `pk_guid`(`guid`) USING BTREE,
  INDEX `pk_publishingDate`(`publishingdate`) USING BTREE,
  INDEX `pk_title`(`title`) USING BTREE,
  INDEX `pk_update`(`update_time`) USING BTREE,
  INDEX `rss_feed_entry_f_id_idx`(`f_id`, `publishingdate`) USING BTREE,
  INDEX `rss_feed_entry_id_idx`(`id`, `f_id`, `publishingdate`) USING BTREE,
  INDEX `rss_feed_entry_publishingdate_idx`(`publishingdate`, `f_id`) USING BTREE,
  FULLTEXT INDEX `full_text`(`title`) WITH PARSER `ngram`
) ENGINE = InnoDB AUTO_INCREMENT = 6870893 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci COMMENT = '这个表 主要用来 存储订阅源 的文章 内容' ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Table structure for rss_feed_user
-- ----------------------------
DROP TABLE IF EXISTS `rss_feed_user`;
CREATE TABLE `rss_feed_user`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `url` varchar(125) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `u_id` int(11) NULL DEFAULT NULL,
  `f_id` int(11) NULL DEFAULT NULL COMMENT '关联feeds表',
  `g_id` int(11) NULL DEFAULT NULL COMMENT '分组id',
  `category` smallint(6) NULL DEFAULT NULL,
  `name` varchar(125) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `icon_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `website` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `description` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `lastupdate` datetime NULL DEFAULT NULL,
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  `min_auto_updatetime` int(11) NOT NULL COMMENT '单位 分钟',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `url`(`url`, `u_id`) USING BTREE,
  INDEX `category`(`category`) USING BTREE,
  INDEX `feed_user_name_index`(`name`) USING BTREE,
  INDEX `pk_rss_feed_user_uid_index`(`u_id`) USING BTREE,
  INDEX `pk_update_time`(`update_time`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 10741 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for rss_feeds
-- ----------------------------
DROP TABLE IF EXISTS `rss_feeds`;
CREATE TABLE `rss_feeds`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `icon_url` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `recommended` bit(1) NOT NULL DEFAULT b'0',
  `is_show` bit(1) NOT NULL DEFAULT b'0',
  `content` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `order` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `create_time` datetime NOT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  `description` text CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL,
  `link` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL COMMENT '源网站的链接',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `pk_url`(`url`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1070 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for rss_group
-- ----------------------------
DROP TABLE IF EXISTS `rss_group`;
CREATE TABLE `rss_group`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `u_id` int(11) NOT NULL,
  `group_name` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `create_time` datetime NULL DEFAULT NULL,
  `update_time` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `pk_u_id`(`u_id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for rss_notice
-- ----------------------------
DROP TABLE IF EXISTS `rss_notice`;
CREATE TABLE `rss_notice`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `text` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for rss_read_log
-- ----------------------------
DROP TABLE IF EXISTS `rss_read_log`;
CREATE TABLE `rss_read_log`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `u_id` int(11) NULL DEFAULT NULL COMMENT '用户id',
  `fe_id` int(11) NULL DEFAULT NULL COMMENT '关联rss_feed_entry',
  `create_date` datetime NULL DEFAULT NULL,
  `update_date` datetime NULL DEFAULT NULL,
  `position` int(11) NOT NULL COMMENT '阅读位置',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `pk_01`(`u_id`, `fe_id`) USING BTREE COMMENT '唯一索引'
) ENGINE = InnoDB AUTO_INCREMENT = 17701 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for rss_subscribe_wechat_message
-- ----------------------------
DROP TABLE IF EXISTS `rss_subscribe_wechat_message`;
CREATE TABLE `rss_subscribe_wechat_message`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `templateid` varchar(125) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `touser_id` int(11) NULL DEFAULT NULL,
  `touser_openid` varchar(125) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `feed_user_id` int(11) NULL DEFAULT NULL COMMENT '关联rss_feed_user订阅表',
  `feeds_id` int(11) NULL DEFAULT NULL COMMENT '关联rss_feeds',
  `feed_entry_id` int(11) NULL DEFAULT NULL COMMENT '关联rss_feed_entry文章表',
  `use_date` datetime NULL DEFAULT NULL COMMENT '使用时间',
  `is_use` bit(1) NULL DEFAULT NULL COMMENT '是否使用（消费）',
  `update_date` datetime NULL DEFAULT NULL,
  `create_date` datetime NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1146 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci COMMENT = '用户订阅推送消息' ROW_FORMAT = Dynamic;

-- ----------------------------
-- Table structure for user
-- ----------------------------
DROP TABLE IF EXISTS `user`;
CREATE TABLE `user`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `password` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `imageurl` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `caeatetime` datetime NULL DEFAULT NULL,
  `openid` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `last_login_time` datetime NULL DEFAULT NULL COMMENT '最后登录时间',
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `pk_openid`(`openid`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 2373 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = Dynamic;

SET FOREIGN_KEY_CHECKS = 1;
