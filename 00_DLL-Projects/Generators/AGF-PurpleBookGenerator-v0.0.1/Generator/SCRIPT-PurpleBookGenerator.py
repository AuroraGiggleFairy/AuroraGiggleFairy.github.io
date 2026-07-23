"""SCRIPT-GenPurpleBook.py
=============================
Generates the AGF Purple Book ``Schematics`` window from vanilla
``progression.xml`` (+ optional mod overlays) plus a small constants module
that supplies the book-cover sprite per crafting_skill.

This pass produces ``craftinglistTab`` (magazines view) while preserving the
established ``booksTab``/``unlockablesTab`` layouts from template/source files.
``armorsTab`` remains an in-progress lane and is preserved from existing output
when available so the resulting mod loads in-game.

Output: ``00_DLL-Projects/AGF-PurpleBookGenerator-vX.Y.Z/Config/XUi_InGame/windows.xml``.

Usage::

    python SCRIPT-GenPurpleBook.py [--out-mod AGF-PurpleBookGenerator-v0.0.1]
                                   [--progression <path>]
"""
from __future__ import annotations

import argparse
import copy
import csv
import datetime
import io
import math
import os
import re
import shutil
import typing
import contextlib
import urllib.request
import json

# Curated translations for short armor set-bonus labels authored in generator
# logic (SetBonus1/SetBonus2). These rows are not sourced directly from
# vanilla localization, so they need explicit multilingual text.
SETBONUS_SHORT_TRANSLATIONS: dict[str, dict[str, str]] = {
    ".44 Damage": {
        "german": ".44-Schaden",
        "spanish": "Daño de .44",
        "french": "Dégâts du .44",
        "italian": "Danno .44",
        "japanese": ".44ダメージ",
        "koreana": ".44 피해량",
        "polish": "Obrażenia .44",
        "brazilian": "Dano .44",
        "russian": "Урон .44",
        "turkish": ".44 Hasarı",
        "schinese": ".44伤害",
        "tchinese": ".44傷害",
    },
    ".44 Reload Speed": {
        "german": ".44-Nachladegeschwindigkeit",
        "spanish": "Velocidad de recarga de .44",
        "french": "Vitesse de rechargement du .44",
        "italian": "Velocità di ricarica .44",
        "japanese": ".44リロード速度",
        "koreana": ".44 재장전 속도",
        "polish": "Szybkość przeładowania .44",
        "brazilian": "Velocidade de recarga da .44",
        "russian": "Скорость перезарядки .44",
        "turkish": ".44 Yeniden Doldurma Hızı",
        "schinese": ".44装填速度",
        "tchinese": ".44換彈速度",
    },
    "Armor Crit Resist": {
        "german": "Rüstungs-Krit-Resistenz",
        "spanish": "Resistencia crítica de armadura",
        "french": "Résistance critique d'armure",
        "italian": "Resistenza critica armatura",
        "japanese": "アーマー致命傷耐性",
        "koreana": "방어구 치명상 저항",
        "polish": "Odporność pancerza na krytyki",
        "brazilian": "Resistência crítica da armadura",
        "russian": "Сопротивление крит. травмам брони",
        "turkish": "Zırh Kritik Direnci",
        "schinese": "护甲暴击抗性",
        "tchinese": "護甲暴擊抗性",
    },
    "Armor Rating": {
        "german": "Rüstungswert",
        "spanish": "Valor de armadura",
        "french": "Valeur d'armure",
        "italian": "Valore armatura",
        "japanese": "アーマー値",
        "koreana": "방어구 수치",
        "polish": "Wartość pancerza",
        "brazilian": "Valor de armadura",
        "russian": "Показатель брони",
        "turkish": "Zırh Değeri",
        "schinese": "护甲值",
        "tchinese": "護甲值",
    },
    "Axe Stamina Cost": {
        "german": "Ausdauerkosten für Äxte",
        "spanish": "Costo de aguante de hachas",
        "french": "Coût en endurance des haches",
        "italian": "Costo stamina asce",
        "japanese": "斧スタミナ消費",
        "koreana": "도끼 스태미나 소모",
        "polish": "Koszt wytrzymałości toporów",
        "brazilian": "Custo de vigor com machados",
        "russian": "Расход выносливости топоров",
        "turkish": "Balta Dayanıklılık Maliyeti",
        "schinese": "斧类体力消耗",
        "tchinese": "斧類耐力消耗",
    },
    "Bike Fuel Use": {
        "german": "Spritverbrauch Motorrad",
        "spanish": "Consumo de combustible de moto",
        "french": "Consommation de carburant moto",
        "italian": "Consumo carburante moto",
        "japanese": "バイク燃料消費",
        "koreana": "바이크 연료 소모",
        "polish": "Zużycie paliwa motocykla",
        "brazilian": "Consumo de combustível da moto",
        "russian": "Расход топлива байка",
        "turkish": "Motosiklet Yakıt Tüketimi",
        "schinese": "摩托燃料消耗",
        "tchinese": "機車燃料消耗",
    },
    "Crit Heal Speed": {
        "german": "Heiltempo kritischer Verletzungen",
        "spanish": "Velocidad de curación de críticos",
        "french": "Vitesse de soin des blessures critiques",
        "italian": "Velocità guarigione critici",
        "japanese": "致命傷回復速度",
        "koreana": "치명상 회복 속도",
        "polish": "Szybkość leczenia urazów krytycznych",
        "brazilian": "Velocidade de cura de críticos",
        "russian": "Скорость лечения крит. травм",
        "turkish": "Kritik İyileşme Hızı",
        "schinese": "重伤恢复速度",
        "tchinese": "重創恢復速度",
    },
    "Crit Resist": {
        "german": "Krit-Resistenz",
        "spanish": "Resistencia a críticos",
        "french": "Résistance critique",
        "italian": "Resistenza ai critici",
        "japanese": "致命傷耐性",
        "koreana": "치명상 저항",
        "polish": "Odporność na krytyki",
        "brazilian": "Resistência a críticos",
        "russian": "Сопротивление крит. травмам",
        "turkish": "Kritik Direnç",
        "schinese": "重伤抗性",
        "tchinese": "重創抗性",
    },
    "Dukes Loot Bonus": {
        "german": "Dukes-Lootbonus",
        "spanish": "Bonificación de botín de Dukes",
        "french": "Bonus de butin de Dukes",
        "italian": "Bonus bottino Dukes",
        "japanese": "デュークス戦利品ボーナス",
        "koreana": "듀크스 전리품 보너스",
        "polish": "Premia do łupów Dukes",
        "brazilian": "Bônus de saque de Dukes",
        "russian": "Бонус к добыче дуков",
        "turkish": "Dukes Ganimet Bonusu",
        "schinese": "公爵币战利品加成",
        "tchinese": "公爵幣戰利品加成",
    },
    "Enemy Search Time": {
        "german": "Gegnersuchzeit",
        "spanish": "Tiempo de búsqueda del enemigo",
        "french": "Durée de recherche ennemie",
        "italian": "Tempo di ricerca nemica",
        "japanese": "敵索敵時間",
        "koreana": "적 탐색 시간",
        "polish": "Czas poszukiwania przez wroga",
        "brazilian": "Tempo de busca do inimigo",
        "russian": "Время поиска врага",
        "turkish": "Düşman Arama Süresi",
        "schinese": "敌人搜索时间",
        "tchinese": "敵人搜索時間",
    },
    "Food Health/Stamina Bonus": {
        "german": "Bonus auf Gesundheit/Ausdauer durch Nahrung",
        "spanish": "Bono de salud/aguante por comida",
        "french": "Bonus santé/endurance de nourriture",
        "italian": "Bonus salute/stamina dal cibo",
        "japanese": "食料の体力/スタミナボーナス",
        "koreana": "음식 체력/스태미나 보너스",
        "polish": "Premia zdrowie/wytrzymałość z jedzenia",
        "brazilian": "Bônus de vida/vigor da comida",
        "russian": "Бонус здоровья/выносливости от еды",
        "turkish": "Yemekten Sağlık/Dayanıklılık Bonusu",
        "schinese": "食物生命/耐力加成",
        "tchinese": "食物生命/耐力加成",
    },
    "Food/Water per Stamina": {
        "german": "Nahrung/Wasser pro Ausdauer",
        "spanish": "Comida/agua por aguante",
        "french": "Nourriture/eau par endurance",
        "italian": "Cibo/acqua per stamina",
        "japanese": "スタミナ当たり食料/水分",
        "koreana": "스태미나당 음식/물",
        "polish": "Jedzenie/woda na wytrzymałość",
        "brazilian": "Comida/água por vigor",
        "russian": "Еда/вода на выносливость",
        "turkish": "Dayanıklılık Başına Yemek/Su",
        "schinese": "每耐力食物/水消耗",
        "tchinese": "每耐力食物/水消耗",
    },
    "Infection Resist": {
        "german": "Infektionsresistenz",
        "spanish": "Resistencia a infección",
        "french": "Résistance à l'infection",
        "italian": "Resistenza infezione",
        "japanese": "感染耐性",
        "koreana": "감염 저항",
        "polish": "Odporność na infekcję",
        "brazilian": "Resistência à infecção",
        "russian": "Сопротивление инфекции",
        "turkish": "Enfeksiyon Direnci",
        "schinese": "感染抗性",
        "tchinese": "感染抗性",
    },
    "Loot Stage Bonus": {
        "german": "Lootstufen-Bonus",
        "spanish": "Bono de nivel de botín",
        "french": "Bonus de niveau de butin",
        "italian": "Bonus livello bottino",
        "japanese": "戦利品ステージボーナス",
        "koreana": "전리품 단계 보너스",
        "polish": "Premia do etapu łupu",
        "brazilian": "Bônus de nível de saque",
        "russian": "Бонус к стадии добычи",
        "turkish": "Ganimet Aşaması Bonusu",
        "schinese": "战利品阶段加成",
        "tchinese": "戰利品階段加成",
    },
    "Mining Tool Durability Loss": {
        "german": "Haltbarkeitsverlust von Bergbauwerkzeugen",
        "spanish": "Pérdida de durabilidad de herramientas de minería",
        "french": "Perte de durabilité des outils de minage",
        "italian": "Perdita durabilità strumenti da miniera",
        "japanese": "採掘ツール耐久消耗",
        "koreana": "채광 도구 내구도 소모",
        "polish": "Utrata trwałości narzędzi górniczych",
        "brazilian": "Perda de durabilidade de ferramentas de mineração",
        "russian": "Потеря прочности шахтерских инструментов",
        "turkish": "Madencilik Aracı Dayanıklılık Kaybı",
        "schinese": "采矿工具耐久损耗",
        "tchinese": "採礦工具耐久損耗",
    },
    "No Set Bonus": {
        "german": "Kein Setbonus",
        "spanish": "Sin bonificación de conjunto",
        "french": "Aucun bonus d'ensemble",
        "italian": "Nessun bonus set",
        "japanese": "セットボーナスなし",
        "koreana": "세트 보너스 없음",
        "polish": "Brak premii za zestaw",
        "brazilian": "Sem bônus de conjunto",
        "russian": "Нет бонуса комплекта",
        "turkish": "Set bonusu yok",
        "schinese": "无套装奖励",
        "tchinese": "無套裝獎勵",
    },
    "none": {
        "german": "keiner",
        "spanish": "ninguno",
        "french": "aucun",
        "italian": "nessuno",
        "japanese": "なし",
        "koreana": "없음",
        "polish": "brak",
        "brazilian": "nenhum",
        "russian": "нет",
        "turkish": "yok",
        "schinese": "无",
        "tchinese": "無",
    },
    "Revolver/Lever Reload Speed": {
        "german": "Nachladegeschwindigkeit Revolver/Hebelgewehr",
        "spanish": "Velocidad de recarga de revólver/de palanca",
        "french": "Vitesse de rechargement revolver/levier",
        "italian": "Velocità ricarica revolver/leva",
        "japanese": "リボルバー/レバー式リロード速度",
        "koreana": "리볼버/레버액션 재장전 속도",
        "polish": "Szybkość przeładowania rewolweru/lever-action",
        "brazilian": "Velocidade de recarga de revólver/alavanca",
        "russian": "Скорость перезарядки револьвера/рычажной винтовки",
        "turkish": "Revolver/Kaldıraçlı Yeniden Doldurma Hızı",
        "schinese": "左轮/杠杆枪装填速度",
        "tchinese": "左輪/槓桿槍換彈速度",
    },
    "Stamina Regen Food/Water Cost": {
        "german": "Nahrung/Wasser-Kosten für Ausdauerregeneration",
        "spanish": "Costo de comida/agua para regenerar aguante",
        "french": "Coût nourriture/eau pour régénération d'endurance",
        "italian": "Costo cibo/acqua per rigenerazione stamina",
        "japanese": "スタミナ回復の食料/水分コスト",
        "koreana": "스태미나 회복 음식/물 소모",
        "polish": "Koszt jedzenia/wody regeneracji wytrzymałości",
        "brazilian": "Custo de comida/água para regenerar vigor",
        "russian": "Расход еды/воды на восстановление выносливости",
        "turkish": "Dayanıklılık Yenileme Yemek/Su Maliyeti",
        "schinese": "耐力恢复食物/水消耗",
        "tchinese": "耐力恢復食物/水消耗",
    },
    "Tools/Weapon Durability Loss": {
        "german": "Haltbarkeitsverlust von Werkzeugen/Waffen",
        "spanish": "Pérdida de durabilidad de herramientas/armas",
        "french": "Perte de durabilité outils/armes",
        "italian": "Perdita durabilità strumenti/armi",
        "japanese": "ツール/武器耐久消耗",
        "koreana": "도구/무기 내구도 소모",
        "polish": "Utrata trwałości narzędzi/broni",
        "brazilian": "Perda de durabilidade de ferramentas/armas",
        "russian": "Потеря прочности инструментов/оружия",
        "turkish": "Alet/Silah Dayanıklılık Kaybı",
        "schinese": "工具/武器耐久损耗",
        "tchinese": "工具/武器耐久損耗",
    },
    "Wood Harvest": {
        "german": "Holzernte",
        "spanish": "Recolección de madera",
        "french": "Récolte de bois",
        "italian": "Raccolta legno",
        "japanese": "木材採取量",
        "koreana": "목재 채집량",
        "polish": "Pozyskiwanie drewna",
        "brazilian": "Coleta de madeira",
        "russian": "Добыча дерева",
        "turkish": "Odun Toplama",
        "schinese": "木材采集量",
        "tchinese": "木材採集量",
    },
}

# Curated translations for generator-authored UI/category localization keys.
LOCALIZATION_KEY_TRANSLATIONS: dict[str, dict[str, str]] = {
    "xuiSchematics": {
        "german": "LILA BUCH",
        "spanish": "LIBRO PÚRPURA",
        "french": "LIVRE VIOLET",
        "italian": "LIBRO VIOLA",
        "japanese": "パープルブック",
        "koreana": "퍼플 북",
        "polish": "FIOLETOWA KSIĘGA",
        "brazilian": "LIVRO ROXO",
        "russian": "ФИОЛЕТОВАЯ КНИГА",
        "turkish": "MOR KITAP",
        "schinese": "紫色书",
        "tchinese": "紫色書",
    },
    "agf0PurpleBookButton": {
        "german": "LILA BUCH",
        "spanish": "LIBRO PÚRPURA",
        "french": "LIVRE VIOLET",
        "italian": "LIBRO VIOLA",
        "japanese": "パープルブック",
        "koreana": "퍼플 북",
        "polish": "FIOLETOWA KSIĘGA",
        "brazilian": "LIVRO ROXO",
        "russian": "ФИОЛЕТОВАЯ КНИГА",
        "turkish": "MOR KITAP",
        "schinese": "紫色书",
        "tchinese": "紫色書",
    },
    "agf0PurpleBookButtonTooltip": {
        "german": "Lila Buch",
        "spanish": "Libro púrpura",
        "french": "Livre violet",
        "italian": "Libro viola",
        "japanese": "パープルブック",
        "koreana": "퍼플 북",
        "polish": "Fioletowa księga",
        "brazilian": "Livro roxo",
        "russian": "Фиолетовая книга",
        "turkish": "Mor Kitap",
        "schinese": "紫色书",
        "tchinese": "紫色書",
    },
    "agf1MainHeaderHint": {
        "german": "Für Details mit der Maus darüberfahren. Zum Hineinzoomen Tabs verwenden.",
        "spanish": "Pasa el cursor para ver detalles. Usa las pestañas para acercar.",
        "french": "Survolez pour voir les détails. Utilisez les onglets pour zoomer.",
        "italian": "Passa il cursore per i dettagli. Usa le schede per ingrandire.",
        "japanese": "詳細はホバーで表示。タブで拡大表示します。",
        "koreana": "자세한 내용은 마우스를 올려 확인하세요. 탭으로 확대합니다.",
        "polish": "Najedź kursorem, aby zobaczyć szczegóły. Użyj kart, aby przybliżyć.",
        "brazilian": "Passe o cursor para ver detalhes. Use as abas para ampliar.",
        "russian": "Наведите курсор для подробностей. Используйте вкладки для увеличения.",
        "turkish": "Ayrıntılar için üzerine gel. Yakınlaştırmak için sekmeleri kullan.",
        "schinese": "悬停查看详情。使用标签页放大查看。",
        "tchinese": "懸停即可查看詳情。使用分頁放大查看。",
    },
    "agf1MainTabArmors": {
        "german": "Rüstungen",
        "spanish": "Armaduras",
        "french": "Armures",
        "italian": "Armature",
        "japanese": "アーマー",
        "koreana": "방어구",
        "polish": "Pancerze",
        "brazilian": "Armaduras",
        "russian": "Броня",
        "turkish": "Zırhlar",
        "schinese": "护甲",
        "tchinese": "護甲",
    },
    "agf1MainTabBooks": {
        "german": "Bücher",
        "spanish": "Libros",
        "french": "Livres",
        "italian": "Libri",
        "japanese": "本",
        "koreana": "책",
        "polish": "Książki",
        "brazilian": "Livros",
        "russian": "Книги",
        "turkish": "Kitaplar",
        "schinese": "书",
        "tchinese": "書籍",
    },
    "agf1MainTabMagazines": {
        "german": "Zeitschriften",
        "spanish": "Revistas",
        "french": "Magazines",
        "italian": "Riviste",
        "japanese": "雑誌",
        "koreana": "잡지",
        "polish": "Czasopisma",
        "brazilian": "Revistas",
        "russian": "Журналы",
        "turkish": "Dergiler",
        "schinese": "杂志",
        "tchinese": "雜誌",
    },
    "agf1MainTabOverview": {
        "german": "Übersicht",
        "spanish": "Resumen",
        "french": "Aperçu",
        "italian": "Panoramica",
        "japanese": "概要",
        "koreana": "개요",
        "polish": "Przegląd",
        "brazilian": "Visão geral",
        "russian": "Обзор",
        "turkish": "Genel Bakış",
        "schinese": "概览",
        "tchinese": "總覽",
    },
    "agf1MainTabOverviewTooltip": {
        "german": "Zur vollständigen Seitenübersicht zurückkehren.",
        "spanish": "Volver a la vista general de página completa.",
        "french": "Revenir à la vue d'ensemble de la page complète.",
        "italian": "Torna alla panoramica completa della pagina.",
        "japanese": "ページ全体の概要に戻ります。",
        "koreana": "전체 페이지 개요로 돌아갑니다.",
        "polish": "Wróć do pełnego podglądu strony.",
        "brazilian": "Retorna à visão geral completa da página.",
        "russian": "Вернуться к полному обзору страницы.",
        "turkish": "Tam sayfa genel görünümüne dön.",
        "schinese": "返回完整页面概览。",
        "tchinese": "返回完整頁面總覽。",
    },
    "agf1MainTabUnlocks": {
        "german": "Freischaltungen",
        "spanish": "Desbloqueos",
        "french": "Déblocages",
        "italian": "Sblocchi",
        "japanese": "アンロック",
        "koreana": "잠금 해제",
        "polish": "Odblokowania",
        "brazilian": "Desbloqueios",
        "russian": "Разблокировки",
        "turkish": "Kilit Açımları",
        "schinese": "解锁",
        "tchinese": "解鎖",
    },
    "agf4UnlocksCategoryAmmo": {
        "german": "Munition",
        "spanish": "Municiones",
        "french": "Munitions",
        "italian": "Munizioni",
        "japanese": "弾薬",
        "koreana": "탄약",
        "polish": "Amunicja",
        "brazilian": "Munição",
        "russian": "Боеприпасы",
        "turkish": "Mühimmat",
        "schinese": "弹药",
        "tchinese": "彈藥",
    },
    "agf4UnlocksCategoryAmmoRow44": {
        "german": ".44-Munition",
        "spanish": "Munición de .44",
        "french": "Munitions .44",
        "italian": "Munizioni .44",
        "japanese": ".44弾薬",
        "koreana": ".44 탄약",
        "polish": "Amunicja .44",
        "brazilian": "Munição .44",
        "russian": "Боеприпасы .44",
        "turkish": ".44 Mühimmat",
        "schinese": ".44弹药",
        "tchinese": ".44彈藥",
    },
    "agf4UnlocksCategoryAmmoRow762": {
        "german": "7.62-Munition",
        "spanish": "Munición de 7.62",
        "french": "Munitions 7.62",
        "italian": "Munizioni 7.62",
        "japanese": "7.62弾薬",
        "koreana": "7.62 탄약",
        "polish": "Amunicja 7.62",
        "brazilian": "Munição 7.62",
        "russian": "Боеприпасы 7.62",
        "turkish": "7.62 Mühimmat",
        "schinese": "7.62弹药",
        "tchinese": "7.62彈藥",
    },
    "agf4UnlocksCategoryAmmoRow9mm": {
        "german": "9mm-Munition",
        "spanish": "Munición de 9 mm",
        "french": "Munitions 9 mm",
        "italian": "Munizioni 9 mm",
        "japanese": "9mm弾薬",
        "koreana": "9mm 탄약",
        "polish": "Amunicja 9 mm",
        "brazilian": "Munição 9 mm",
        "russian": "Боеприпасы 9 мм",
        "turkish": "9mm Mühimmat",
        "schinese": "9毫米弹药",
        "tchinese": "9毫米彈藥",
    },
    "agf4UnlocksCategoryAmmoRowArrows": {
        "german": "Pfeile",
        "spanish": "Flechas",
        "french": "Flèches",
        "italian": "Frecce",
        "japanese": "矢",
        "koreana": "화살",
        "polish": "Strzały",
        "brazilian": "Flechas",
        "russian": "Стрелы",
        "turkish": "Oklar",
        "schinese": "箭矢",
        "tchinese": "箭矢",
    },
    "agf4UnlocksCategoryAmmoRowBolts": {
        "german": "Bolzen",
        "spanish": "Saetas",
        "french": "Carreaux",
        "italian": "Dardi",
        "japanese": "ボルト",
        "koreana": "볼트",
        "polish": "Bełty",
        "brazilian": "Virotes",
        "russian": "Болты",
        "turkish": "Cıvatalar",
        "schinese": "弩箭",
        "tchinese": "弩矢",
    },
    "agf4UnlocksCategoryAmmoRowJunkTurret": {
        "german": "Junk-Turret-Munition",
        "spanish": "Munición de torreta chatarra",
        "french": "Munitions de tourelle ferraille",
        "italian": "Munizioni torretta junk",
        "japanese": "ジャンクタレット弾薬",
        "koreana": "정크 터렛 탄약",
        "polish": "Amunicja wieżyczki złomu",
        "brazilian": "Munição da torreta junk",
        "russian": "Боеприпасы для турели-хлама",
        "turkish": "Hurda Taret Mühimmatı",
        "schinese": "废料炮塔弹药",
        "tchinese": "廢料砲塔彈藥",
    },
    "agf4UnlocksCategoryAmmoRowRockets": {
        "german": "Raketen",
        "spanish": "Cohetes",
        "french": "Roquettes",
        "italian": "Razzi",
        "japanese": "ロケット",
        "koreana": "로켓",
        "polish": "Rakiety",
        "brazilian": "Foguetes",
        "russian": "Ракеты",
        "turkish": "Roketler",
        "schinese": "火箭弹",
        "tchinese": "火箭彈",
    },
    "agf4UnlocksCategoryAmmoRowShotgun": {
        "german": "Schrotmunition",
        "spanish": "Munición de escopeta",
        "french": "Munitions de fusil à pompe",
        "italian": "Munizioni fucile a pompa",
        "japanese": "ショットガン弾薬",
        "koreana": "산탄총 탄약",
        "polish": "Amunicja do strzelby",
        "brazilian": "Munição de espingarda",
        "russian": "Патроны для дробовика",
        "turkish": "Pompalı Tüfek Mühimmatı",
        "schinese": "霰弹枪弹药",
        "tchinese": "霰彈槍彈藥",
    },
    "agf4UnlocksCategoryArmors": {
        "german": "Rüstungen",
        "spanish": "Armaduras",
        "french": "Armures",
        "italian": "Armature",
        "japanese": "アーマー",
        "koreana": "방어구",
        "polish": "Pancerze",
        "brazilian": "Armaduras",
        "russian": "Броня",
        "turkish": "Zırhlar",
        "schinese": "护甲",
        "tchinese": "護甲",
    },
    "agf4UnlocksCategoryArmorsBoots": {
        "german": "Stiefel",
        "spanish": "Botas",
        "french": "Bottes",
        "italian": "Stivali",
        "japanese": "ブーツ",
        "koreana": "부츠",
        "polish": "Buty",
        "brazilian": "Botas",
        "russian": "Ботинки",
        "turkish": "Çizmeler",
        "schinese": "靴子",
        "tchinese": "靴子",
    },
    "agf4UnlocksCategoryArmorsFittings": {
        "german": "Fittings",
        "spanish": "Ajustes",
        "french": "Raccords",
        "italian": "Finiture",
        "japanese": "接合部",
        "koreana": "피팅",
        "polish": "Połączenia",
        "brazilian": "Encaixes",
        "russian": "Соединения",
        "turkish": "Donanım",
        "schinese": "配件",
        "tchinese": "配件",
    },
    "agf4UnlocksCategoryArmorsGeneral": {
        "german": "Allgemein",
        "spanish": "General",
        "french": "Général",
        "italian": "Generale",
        "japanese": "一般",
        "koreana": "일반",
        "polish": "Ogólne",
        "brazilian": "Geral",
        "russian": "Общее",
        "turkish": "Genel",
        "schinese": "一般",
        "tchinese": "一般",
    },
    "agf4UnlocksCategoryArmorsMuffledConnectors": {
        "german": "Gedämpfte Verbindungen",
        "spanish": "Conectores de amortiguación",
        "french": "Connecteurs étouffés",
        "italian": "Connettori attutiti",
        "japanese": "静音コネクター",
        "koreana": "소음 방지 연결 장치",
        "polish": "Wytłumione połączenia",
        "brazilian": "Conectores abafados",
        "russian": "Бесшумные соединения",
        "turkish": "Sessiz Bağlantılar",
        "schinese": "消音连接器",
        "tchinese": "消音器",
    },
    "agf4UnlocksCategoryArmorsPockets": {
        "german": "Taschen",
        "spanish": "Bolsillos",
        "french": "Poches",
        "italian": "Tasche",
        "japanese": "ポケット",
        "koreana": "주머니",
        "polish": "Kieszenie",
        "brazilian": "Bolsos",
        "russian": "Карманы",
        "turkish": "Cepler",
        "schinese": "口袋",
        "tchinese": "口袋",
    },
    "agf4UnlocksCategoryMisc": {
        "german": "Diverses",
        "spanish": "Misceláneo",
        "french": "Divers",
        "italian": "Varie",
        "japanese": "その他",
        "koreana": "기타",
        "polish": "Różne",
        "brazilian": "Diversos",
        "russian": "Прочее",
        "turkish": "Diğer",
        "schinese": "其他",
        "tchinese": "雜項",
    },
    "agf4UnlocksCategoryToolsWeapons": {
        "german": "Werkzeuge & Waffen",
        "spanish": "Herramientas y armas",
        "french": "Outils et armes",
        "italian": "Attrezzi e armi",
        "japanese": "ツールと武器",
        "koreana": "도구 및 무기",
        "polish": "Narzędzia i broń",
        "brazilian": "Ferramentas e armas",
        "russian": "Инструменты и оружие",
        "turkish": "Aletler ve Silahlar",
        "schinese": "工具和武器",
        "tchinese": "工具與武器",
    },
    "agf4UnlocksCategoryToolsWeaponsBarrels": {
        "german": "Läufe",
        "spanish": "Cañones",
        "french": "Canons",
        "italian": "Canne",
        "japanese": "バレル",
        "koreana": "총열",
        "polish": "Lufy",
        "brazilian": "Canos",
        "russian": "Стволы",
        "turkish": "Namlular",
        "schinese": "枪管",
        "tchinese": "槍管",
    },
    "agf4UnlocksCategoryToolsWeaponsBlockDamage": {
        "german": "Blockschaden",
        "spanish": "Daño en bloque",
        "french": "Dégâts de bloc",
        "italian": "Danno ai blocchi",
        "japanese": "対物ダメージ",
        "koreana": "블록 대미지",
        "polish": "Obrażenia dla bloku",
        "brazilian": "Dano a bloco",
        "russian": "Урон блока",
        "turkish": "Blok Hasarı",
        "schinese": "物块伤害",
        "tchinese": "塊損壞",
    },
    "agf4UnlocksCategoryToolsWeaponsScopes": {
        "german": "Zielfernrohre",
        "spanish": "Miras",
        "french": "Lunettes",
        "italian": "Mirini",
        "japanese": "スコープ",
        "koreana": "스코프",
        "polish": "Lunety",
        "brazilian": "Miras",
        "russian": "Прицелы",
        "turkish": "Dürbünler",
        "schinese": "瞄准镜",
        "tchinese": "瞄準鏡",
    },
    "agf4UnlocksCategoryToolsWeaponsSpecial": {
        "german": "Spezial",
        "spanish": "Especial",
        "french": "Spécial",
        "italian": "Speciale",
        "japanese": "特殊",
        "koreana": "특수",
        "polish": "Specjalne",
        "brazilian": "Especial",
        "russian": "Особое",
        "turkish": "Özel",
        "schinese": "特殊",
        "tchinese": "特殊",
    },
    "agf4UnlocksCategoryVehicles": {
        "german": "Fahrzeuge",
        "spanish": "Vehículos",
        "french": "Véhicules",
        "italian": "Veicoli",
        "japanese": "車両",
        "koreana": "차량",
        "polish": "Pojazdy",
        "brazilian": "Veículos",
        "russian": "Транспортные средства",
        "turkish": "Araçlar",
        "schinese": "载具",
        "tchinese": "車輛",
    },
    "agf4UnlocksCategoryArmorsHelmet": {
        "german": "Helme",
        "spanish": "Cascos",
        "french": "Casques",
        "italian": "Elmetti",
        "japanese": "ヘルメット",
        "koreana": "헬멧",
        "polish": "Hełmy",
        "brazilian": "Capacetes",
        "russian": "Шлемы",
        "turkish": "Kasklar",
        "schinese": "头盔",
        "tchinese": "頭盔",
    },
    "agf4UnlocksCategoryArmorsPlating": {
        "german": "Panzerplatten",
        "spanish": "Blindajes",
        "french": "Blindages",
        "italian": "Corazze",
        "japanese": "装甲プレート",
        "koreana": "장갑판",
        "polish": "Płyty pancerza",
        "brazilian": "Blindagens",
        "russian": "Бронепластины",
        "turkish": "Zırh Kaplamaları",
        "schinese": "护甲板",
        "tchinese": "護甲板",
    },
    "agf4UnlocksCategoryArmorsMuffledConnectors": {
        "german": "Gedämpfte Verbindungen",
        "spanish": "Conectores amortiguados",
        "french": "Connecteurs étouffés",
        "italian": "Connettori attutiti",
        "japanese": "静音コネクター",
        "koreana": "소음 방지 연결 장치",
        "polish": "Wytłumione połączenia",
        "brazilian": "Conectores abafados",
        "russian": "Бесшумные соединения",
        "turkish": "Sessiz Bağlantılar",
        "schinese": "消音连接器",
        "tchinese": "消音連接器",
    },
    "agf4UnlocksCategoryArmorsPockets": {
        "german": "Taschen",
        "spanish": "Bolsillos",
        "french": "Poches",
        "italian": "Tasche",
        "japanese": "ポケット",
        "koreana": "주머니",
        "polish": "Kieszenie",
        "brazilian": "Bolsos",
        "russian": "Карманы",
        "turkish": "Cepler",
        "schinese": "口袋",
        "tchinese": "口袋",
    },
    "agf4UnlocksCategoryDrone": {
        "german": "Drohnen",
        "spanish": "Drones",
        "french": "Drones",
        "italian": "Droni",
        "japanese": "ドローン",
        "koreana": "드론",
        "polish": "Drony",
        "brazilian": "Drones",
        "russian": "Дроны",
        "turkish": "Dronlar",
        "schinese": "无人机",
        "tchinese": "無人機",
    },
    "agf4UnlocksCategoryMisc": {
        "german": "Verschiedenes",
        "spanish": "Misceláneo",
        "french": "Divers",
        "italian": "Varie",
        "japanese": "その他",
        "koreana": "기타",
        "polish": "Różne",
        "brazilian": "Diversos",
        "russian": "Прочее",
        "turkish": "Diğer",
        "schinese": "其他",
        "tchinese": "雜項",
    },
    "agf4UnlocksCategoryToolsWeapons": {
        "german": "Werkzeuge & Waffen",
        "spanish": "Herramientas y armas",
        "french": "Outils et armes",
        "italian": "Attrezzi e armi",
        "japanese": "ツール/武器",
        "koreana": "도구/무기",
        "polish": "Narzędzia i broń",
        "brazilian": "Ferramentas e armas",
        "russian": "Инструменты и оружие",
        "turkish": "Aletler ve Silahlar",
        "schinese": "工具和武器",
        "tchinese": "工具與武器",
    },
    "agf4UnlocksCategoryToolsWeaponsBarrels": {
        "german": "Läufe",
        "spanish": "Cañones",
        "french": "Canons",
        "italian": "Canne",
        "japanese": "銃身",
        "koreana": "총열",
        "polish": "Lufy",
        "brazilian": "Canos",
        "russian": "Стволы",
        "turkish": "Namlular",
        "schinese": "枪管",
        "tchinese": "槍管",
    },
    "agf4UnlocksCategoryToolsWeaponsBlockDamage": {
        "german": "Blockschaden",
        "spanish": "Daño en bloque",
        "french": "Dégâts de bloc",
        "italian": "Danno ai blocchi",
        "japanese": "対物ダメージ",
        "koreana": "블록 대미지",
        "polish": "Obrażenia dla bloku",
        "brazilian": "Dano a bloco",
        "russian": "Урон блока",
        "turkish": "Blok Hasarı",
        "schinese": "物块伤害",
        "tchinese": "塊損壞",
    },
    "agf4UnlocksCategoryToolsWeaponsClub": {
        "german": "Knüppel",
        "spanish": "Garrotes",
        "french": "Massues",
        "italian": "Mazze",
        "japanese": "棍棒",
        "koreana": "곤봉",
        "polish": "Pałki",
        "brazilian": "Porretes",
        "russian": "Дубинки",
        "turkish": "Sopalar",
        "schinese": "棍棒",
        "tchinese": "棍棒",
    },
    "agf4UnlocksCategoryToolsWeaponsMotorTool": {
        "german": "Motorwerkzeuge",
        "spanish": "Herramientas motorizadas",
        "french": "Outils motorisés",
        "italian": "Strumenti motorizzati",
        "japanese": "モーターツール",
        "koreana": "모터 도구",
        "polish": "Narzędzia silnikowe",
        "brazilian": "Ferramentas motorizadas",
        "russian": "Моторизированные инструменты",
        "turkish": "Motorlu Aletler",
        "schinese": "机动工具",
        "tchinese": "動力工具",
    },
    "agf4UnlocksCategoryToolsWeaponsOtherGun": {
        "german": "Fernkampf (Allgemein)",
        "spanish": "A distancia (general)",
        "french": "Distance (général)",
        "italian": "Distanza (generale)",
        "japanese": "遠距離（一般）",
        "koreana": "원거리(일반)",
        "polish": "Dystansowe (ogólne)",
        "brazilian": "Longo alcance (geral)",
        "russian": "Дальний бой (общее)",
        "turkish": "Menzilli (Genel)",
        "schinese": "远程（通用）",
        "tchinese": "遠程（通用）",
    },
    "agf4UnlocksCategoryToolsWeaponsOtherMelee": {
        "german": "Nahkampf (Allgemein)",
        "spanish": "Cuerpo a cuerpo (general)",
        "french": "Mêlée (général)",
        "italian": "Mischia (generale)",
        "japanese": "近接（一般）",
        "koreana": "근접(일반)",
        "polish": "Wręcz (ogólne)",
        "brazilian": "Corpo a corpo (geral)",
        "russian": "Ближний бой (общее)",
        "turkish": "Yakın Dövüş (Genel)",
        "schinese": "近战（通用）",
        "tchinese": "近戰（通用）",
    },
    "agf4UnlocksCategoryToolsWeaponsShotgun": {
        "german": "Schrotflinten",
        "spanish": "Escopetas",
        "french": "Fusils à pompe",
        "italian": "Fucili a pompa",
        "japanese": "ショットガン",
        "koreana": "산탄총",
        "polish": "Strzelby",
        "brazilian": "Escopetas",
        "russian": "Дробовики",
        "turkish": "Pompalı Tüfekler",
        "schinese": "霰弹枪",
        "tchinese": "霰彈槍",
    },
    "agf4UnlocksCategoryToolsWeaponsScopes": {
        "german": "Zielfernrohre",
        "spanish": "Miras",
        "french": "Lunettes de visée",
        "italian": "Mirini",
        "japanese": "スコープ",
        "koreana": "스코프",
        "polish": "Lunety",
        "brazilian": "Miras",
        "russian": "Прицелы",
        "turkish": "Dürbünler",
        "schinese": "瞄准镜",
        "tchinese": "瞄準鏡",
    },
    "agf4UnlocksCategoryToolsWeaponsSpecial": {
        "german": "Spezial",
        "spanish": "Especial",
        "french": "Spécial",
        "italian": "Speciale",
        "japanese": "特殊",
        "koreana": "특수",
        "polish": "Specjalne",
        "brazilian": "Especial",
        "russian": "Особое",
        "turkish": "Özel",
        "schinese": "特殊",
        "tchinese": "特殊",
    },
    "agf4UnlocksCategoryVehicles": {
        "german": "Fahrzeuge",
        "spanish": "Vehículos",
        "french": "Véhicules",
        "italian": "Veicoli",
        "japanese": "車両",
        "koreana": "차량",
        "polish": "Pojazdy",
        "brazilian": "Veículos",
        "russian": "Транспорт",
        "turkish": "Araçlar",
        "schinese": "交通工具",
        "tchinese": "車輛",
    },
    "agf5ArmorsRatingHeavy": {
        "german": "Schwere Rüstungen",
        "spanish": "Armaduras pesadas",
        "french": "Armures lourdes",
        "italian": "Armature pesanti",
        "japanese": "ヘビーアーマー",
        "koreana": "중갑옷",
        "polish": "Ciężkie pancerze",
        "brazilian": "Armaduras pesadas",
        "russian": "Тяжелая броня",
        "turkish": "Ağır Zırhlar",
        "schinese": "重甲",
        "tchinese": "重甲",
    },
    "agf5ArmorsRatingLight": {
        "german": "Leichte Rüstungen",
        "spanish": "Armaduras ligeras",
        "french": "Armures légères",
        "italian": "Armature leggere",
        "japanese": "ライトアーマー",
        "koreana": "경갑옷",
        "polish": "Lekkie pancerze",
        "brazilian": "Armaduras leves",
        "russian": "Легкая броня",
        "turkish": "Hafif Zırhlar",
        "schinese": "轻甲",
        "tchinese": "輕甲",
    },
    "agf5ArmorsRatingMedium": {
        "german": "Mittlere Rüstungen",
        "spanish": "Armaduras medias",
        "french": "Armures moyennes",
        "italian": "Armature medie",
        "japanese": "ミディアムアーマー",
        "koreana": "중형 갑옷",
        "polish": "Średnie pancerze",
        "brazilian": "Armaduras médias",
        "russian": "Средняя броня",
        "turkish": "Orta Zırhlar",
        "schinese": "中甲",
        "tchinese": "中甲",
    },
    "agf5ArmorsRatingHeavy": {
        "german": "Schwere Rüstungen",
        "spanish": "Armaduras pesadas",
        "french": "Armures lourdes",
        "italian": "Armature pesanti",
        "japanese": "ヘビーアーマー",
        "koreana": "중갑옷",
        "polish": "Ciężkie pancerze",
        "brazilian": "Armaduras pesadas",
        "russian": "Тяжелая броня",
        "turkish": "Ağır Zırhlar",
        "schinese": "重甲",
        "tchinese": "重甲",
    },
    "agf5ArmorsRatingLight": {
        "german": "Leichte Rüstungen",
        "spanish": "Armaduras ligeras",
        "french": "Armures légères",
        "italian": "Armature leggere",
        "japanese": "ライトアーマー",
        "koreana": "경갑옷",
        "polish": "Lekkie pancerze",
        "brazilian": "Armaduras leves",
        "russian": "Легкая броня",
        "turkish": "Hafif Zırhlar",
        "schinese": "轻甲",
        "tchinese": "輕甲",
    },
    "agf5ArmorsRatingMedium": {
        "german": "Mittlere Rüstungen",
        "spanish": "Armaduras medias",
        "french": "Armures intermédiaires",
        "italian": "Armature medie",
        "japanese": "ミディアムアーマー",
        "koreana": "중형 갑옷",
        "polish": "Średnie pancerze",
        "brazilian": "Armaduras médias",
        "russian": "Средняя броня",
        "turkish": "Orta Zırhlar",
        "schinese": "中甲",
        "tchinese": "中甲",
    },
    "agf5ArmorSetBonusTooltip": {
        "german": "Setbonus",
        "spanish": "Bonificación de conjunto",
        "french": "Bonus d'ensemble",
        "italian": "Bonus set",
        "japanese": "セットボーナス",
        "koreana": "세트 보너스",
        "polish": "Premia za zestaw",
        "brazilian": "Bônus de conjunto",
        "russian": "Бонус комплекта",
        "turkish": "Set Bonusu",
        "schinese": "套装奖励",
        "tchinese": "套裝獎勵",
    },
}


def _translate_setbonus_short_label(english_text: str, target_lang: str) -> str:
    key = (english_text or "").strip()
    if not key:
        return ""
    by_lang = SETBONUS_SHORT_TRANSLATIONS.get(key)
    if not by_lang:
        return ""
    return (by_lang.get(target_lang) or "").strip()


def _translate_localization_key_label(key: str, target_lang: str) -> str:
    key_name = (key or "").strip()
    if not key_name:
        return ""
    by_lang = LOCALIZATION_KEY_TRANSLATIONS.get(key_name)
    if not by_lang:
        return ""
    return (by_lang.get(target_lang) or "").strip()


def _auto_translate(text: str, target_lang: str) -> str:
    """Placeholder for automatic translation (disabled for quality safety)."""
    return ""

def _fill_missing_translations(row: list[str], lang_indices: dict[str, int], english: str) -> list[str]:
    """Fill missing translation columns using curated mappings only."""
    out = row[:]
    key = (row[0] if row else "").strip()
    is_setbonus_short = bool(re.match(r"^agf(?:5)?Armor[A-Za-z0-9_]+SetBonus[12]$", key))
    for lang, idx in lang_indices.items():
        if idx < len(out) and (not out[idx] or out[idx].strip() == ""):
            mapped_key = _translate_localization_key_label(key, lang)
            if mapped_key:
                out[idx] = mapped_key
                continue
            if is_setbonus_short:
                mapped = _translate_setbonus_short_label(english, lang)
                if mapped:
                    out[idx] = mapped
                    continue
            fallback = _auto_translate(english, lang)
            if fallback:
                out[idx] = fallback
    return out
import sys
import xml.etree.ElementTree as ET
from dataclasses import dataclass, field
from pathlib import Path

# ---------------------------------------------------------------------------
# Paths & constants
# ---------------------------------------------------------------------------

def _detect_workspace() -> Path:
    """Resolve the repository root regardless of launcher location."""
    here = Path(__file__).resolve()
    for parent in here.parents:
        if (parent / "SCRIPT-MakeNewMod.py").exists():
            return parent
    # Fallback keeps older behavior if structure changes.
    return here.parent


WORKSPACE = _detect_workspace()
GENERATOR_MOD_DIR = Path(__file__).resolve().parent.parent
GAME_CONFIG = Path(r"c:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Data\Config")
DEFAULT_PROGRESSION = GAME_CONFIG / "progression.xml"
DEFAULT_QUALITYINFO = GAME_CONFIG / "qualityinfo.xml"
DEFAULT_ITEMS = GAME_CONFIG / "items.xml"
DEFAULT_BLOCKS = GAME_CONFIG / "blocks.xml"
DEFAULT_RECIPES = GAME_CONFIG / "recipes.xml"
DEFAULT_BUFFS = GAME_CONFIG / "buffs.xml"
DEFAULT_GAME_LOCALIZATION = GAME_CONFIG / "Localization.csv"
DEFAULT_OUT_MOD = GENERATOR_MOD_DIR.name
DEFAULT_SEED_WINDOWS = WORKSPACE / "windowBackup.xml"
DEFAULT_RELEASE_WINDOWS = WORKSPACE / "03_ReleaseSource" / "AGF-HUDPlus-PurpleBook-v2.0.1" / "Config" / "XUi_InGame" / "windows.xml"
DEFAULT_RELEASE_BUFFS = WORKSPACE / "03_ReleaseSource" / "AGF-HUDPlus-PurpleBook-v2.0.1" / "Config" / "buffs.xml"
DEFAULT_ACTIVEBUILD_WINDOWS = WORKSPACE / "02_ActiveBuild" / "AGF-HUDPlus-PurpleBook-v2.0.1" / "Config" / "XUi_InGame" / "windows.xml"
DEFAULT_GAME_MOD_WINDOWS = Path(r"c:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods") / DEFAULT_OUT_MOD / "Config" / "XUi_InGame" / "windows.xml"
DEFAULT_BOOKS_TEMPLATE = GENERATOR_MOD_DIR / "Generator" / "TEMPLATE-BooksTab.xml"
DEFAULT_UNLOCKS_TEMPLATE = GENERATOR_MOD_DIR / "Generator" / "TEMPLATE-UnlockablesTab.xml"
DEFAULT_UNLOCK_REVIEW_LEDGER = GENERATOR_MOD_DIR / "Generator" / "UnlockReviewLedger.json"
DEFAULT_ARMOR_COMPACT_EXAMPLE = GENERATOR_MOD_DIR / "Generator" / "EXAMPLE-ArmorCard-Biker-Compact.xml"


def _resolve_out_mod_dir(out_mod_arg: str) -> Path:
    candidate = Path(out_mod_arg)
    if candidate.is_absolute():
        return candidate

    workspace_relative = WORKSPACE / candidate
    if workspace_relative.exists():
        return workspace_relative

    for parent_dir in (WORKSPACE / "00_DLL-Projects" / "Generators", WORKSPACE / "01_Draft"):
        resolved = parent_dir / out_mod_arg
        if resolved.exists():
            return resolved

    if out_mod_arg.lower() == GENERATOR_MOD_DIR.name.lower():
        return GENERATOR_MOD_DIR

    return (WORKSPACE / "00_DLL-Projects" / "Generators" / out_mod_arg)

LOCALIZATION_LANGUAGE_HEADERS = {
    "english",
    "german",
    "spanish",
    "french",
    "italian",
    "japanese",
    "koreana",
    "polish",
    "brazilian",
    "russian",
    "turkish",
    "schinese",
    "tchinese",
}

DEFAULT_LOCALIZATION_HEADER_FIELDS = [
    "Key",
    "File",
    "Type",
    "UsedInMainMenu",
    "NoTranslate",
    "KeepLoaded",
    "english",
    "Context / Alternate Text",
    "german",
    "spanish",
    "french",
    "italian",
    "japanese",
    "koreana",
    "polish",
    "brazilian",
    "russian",
    "turkish",
    "schinese",
    "tchinese",
]

# Window frame
# panel="Left" auto-stacks windows: the <window>'s own pos attribute is
# IGNORED for layout (XUi.LayoutWindowsInPanel sets localPosition.y per
# window in stacking order). Vertical placement of the entire panel is
# controlled by <window_group stack_panel_y_offset="..."> in xui.xml,
# whose default is 457 (NGUI: smaller value = panel sits lower on screen).
WIN_WIDTH = 1390
WIN_HEIGHT = 720
STACK_PANEL_Y_OFFSET = 397  # default 457 minus 60 px to clear page-header band
MAG_STRIP_TOP_Y = -10
MAG_STRIP_HEIGHT = 100  # the 23-icon row at the top of every zoomed view
USABLE_TOP_Y = MAG_STRIP_TOP_Y - MAG_STRIP_HEIGHT  # -110
USABLE_CENTER_Y = (USABLE_TOP_Y - WIN_HEIGHT) // 2  # = -475

# Zoomed armor-piece description layout constants (must stay in sync between
# windows.xml generation and localization line-break fitting).
ZOOM_ARMOR_CARD_WIDTH = 810
ZOOM_ARMOR_DESC_ICON_COL_W = 40
ZOOM_ARMOR_DESC_LABEL_MARGIN_PX = 20
ZOOM_ARMOR_DESC_FONT_SIZE = 18
ZOOM_ARMOR_DESC_LABEL_WIDTH = max(
    180,
    (ZOOM_ARMOR_CARD_WIDTH - ZOOM_ARMOR_DESC_ICON_COL_W) - ZOOM_ARMOR_DESC_LABEL_MARGIN_PX,
)

# Per-sprite tweaks for the Food compact section. Keys = sprite or item name.
# size: int (overrides 13). dx, dy: ints added to default position.
# Bottom-alignment to y=-30 is preserved automatically when size changes.
FOOD_ICON_OVERRIDES: dict[str, dict] = {
    "drinkJarBeer": dict(dy=-2),
}

# Force tooltip text on specific items (overrides entity-name fallback).
# Used for the armor row icons which are representatives for a whole class.
TOOLTIP_OVERRIDES: dict[str, str] = {
    "armorAthleticOutfit": "agf5ArmorsRatingLight",
    "armorAssassinOutfit": "agf5ArmorsRatingMedium",
    "armorMinerOutfit":    "agf5ArmorsRatingHeavy",
}

# Explicit armor-class icon mapping for overview and zoom card headers.
# Sprite names are aligned to base-game progression/perk icon usage.
ARMOR_TYPE_ICON_BY_OUTFIT: dict[str, str] = {
    "armorPrimitiveOutfit": "ui_game_symbol_light_armor",
    "armorAthleticOutfit": "ui_game_symbol_light_armor",
    "armorEnforcerOutfit": "ui_game_symbol_light_armor",
    "armorLumberjackOutfit": "ui_game_symbol_light_armor",
    "armorPreacherOutfit": "ui_game_symbol_light_armor",
    "armorRogueOutfit": "ui_game_symbol_light_armor",
    "armorScavengerOutfit": "ui_game_symbol_light_armor2",
    "armorAssassinOutfit": "ui_game_symbol_light_armor2",
    "armorBikerOutfit": "ui_game_symbol_light_armor2",
    "armorCommandoOutfit": "ui_game_symbol_light_armor2",
    "armorFarmerOutfit": "ui_game_symbol_light_armor2",
    "armorRangerOutfit": "ui_game_symbol_light_armor2",
    "armorMinerOutfit": "ui_game_symbol_armor_iron",
    "armorNerdOutfit": "ui_game_symbol_armor_iron",
    "armorNomadOutfit": "ui_game_symbol_armor_iron",
    "armorRaiderOutfit": "ui_game_symbol_armor_iron",
}


def _enforce_armor_header_icons(armors_rect_xml: str) -> str:
    """Apply per-outfit armor class icons to all armor card header sprites."""
    try:
        root = ET.fromstring(f"<root>{armors_rect_xml}</root>")
    except ET.ParseError:
        return armors_rect_xml

    changed = False
    for grid in root.findall(".//grid"):
        gname = grid.attrib.get("name", "")
        if not (gname.startswith("armor") and gname.endswith("Outfit")):
            continue

        icon_sprite = ARMOR_TYPE_ICON_BY_OUTFIT.get(gname, "")
        if not icon_sprite:
            continue

        icon_armor = grid.find("./entry[@name='1icon']/sprite[@name='iconArmor']")
        if icon_armor is None:
            icon_armor = grid.find(".//sprite[@name='iconArmor']")
        if icon_armor is None:
            continue

        if icon_armor.attrib.get("sprite") != icon_sprite:
            icon_armor.set("sprite", icon_sprite)
            changed = True
        if "atlas" in icon_armor.attrib:
            icon_armor.attrib.pop("atlas", None)
            changed = True

    if not changed:
        return armors_rect_xml
    return "".join(ET.tostring(child, encoding="unicode") for child in list(root))

# Optional per-armor overrides for set-bonus value cells shown on row 6
# (q1 and q6 columns in compact armor cards).
ARMOR_SETBONUS_VALUE_OVERRIDES: dict[str, tuple[str, str]] = {
    "armorAssassinOutfit": ("-15%", "-100%"),
    "armorAthleticOutfit": ("-10%", "-60%"),
    "armorMinerOutfit": ("-10%", "-35%"),
    "armorNerdOutfit": ("-10%", "-35%"),
    "armorNomadOutfit": ("-5%", "-30%"),
}

# Optional explicit per-entry overrides for set-bonus rows. Keys are entry names
# like 6q1/6q6/7q1/7q6.
ARMOR_SETBONUS_ROW_VALUE_OVERRIDES: dict[str, dict[str, str]] = {
    "armorBikerOutfit": {
        "6q1": "1",
        "6q6": "6",
        "7q1": "-2%",
        "7q6": "-20%",
    },
    "armorEnforcerOutfit": {
        "7q1": "5%",
        "7q6": "50%",
    },
    "armorLumberjackOutfit": {
        "7q1": "x2",
        "7q6": "x2",
    },
}

# Explicit per-tier q1..q6 values for split-stat/manual set-bonus rows.
# Hard rule: these are source/authoritative values, never interpolated.
ARMOR_SETBONUS_ROW_SERIES_OVERRIDES: dict[str, dict[int, list[str]]] = {
    "armorBikerOutfit": {
        6: ["1", "2", "3", "4", "5", "6"],
        7: ["-2%", "-4%", "-6%", "-8%", "-10%", "-20%"],
    },
    "armorEnforcerOutfit": {
        6: ["5%", "10%", "15%", "20%", "25%", "50%"],
        7: ["5%", "10%", "15%", "20%", "25%", "50%"],
    },
    "armorLumberjackOutfit": {
        6: ["5%", "10%", "15%", "20%", "25%", "30%"],
        7: ["x2", "x2", "x2", "x2", "x2", "x2"],
    },
    "armorPreacherOutfit": {
        6: ["5%", "10%", "15%", "20%", "25%", "40%"],
        7: ["5%", "10%", "15%", "20%", "25%", "100%"],
    },
}

# Cards that must keep both set-bonus rows visible.
ARMOR_FORCE_DUAL_SETBONUS: set[str] = {
    "armorBikerOutfit",
    "armorEnforcerOutfit",
    "armorLumberjackOutfit",
}

ARMOR_SETBONUS_TOOLTIP_KEY = "agf5ArmorSetBonusTooltip"

# Quality colors (Q1..Q6)
QCOLORS = [
    "156, 136, 103",  # Q1 brown
    "207, 119, 41",   # Q2 orange
    "162, 164, 19",   # Q3 yellow
    "50, 194, 52",    # Q4 green
    "49, 93, 206",    # Q5 blue
    "164, 42, 204",   # Q6 purple
]

# Per-skill book-cover sprite (lifted from existing Purple Book v2.0.1)
BOOK_SPRITE: dict[str, str] = {
    "craftingArmor":           "bookArmoredUp",
    "craftingBlades":          "bookKnifeGuy",
    "craftingBows":            "bookBowHunters",
    "craftingClubs":           "bookBigHitters",
    "craftingElectrician":     "bookWiring101",
    "craftingExplosives":      "bookExplosiveMagazine",
    "craftingFood":            "bookHomeCookingWeekly",
    "craftingHandguns":        "bookHandgunMagazine",
    "craftingHarvestingTools": "bookToolsDigest",
    "craftingKnuckles":        "bookFuriousFists",
    "craftingMachineGuns":     "bookTacticalWarfare",
    "craftingMedical":         "bookMedicalJournal",
    "craftingRepairTools":     "bookHandyLand",
    "craftingRifles":          "bookRifleWorld",
    "craftingRobotics":        "bookTechPlanet",
    "craftingSalvageTools":    "bookScrapping4Fun",
    "craftingSeeds":           "bookSouthernFarming",
    "craftingShotguns":        "bookShotgunWeekly",
    "craftingSledgehammers":   "bookGetHammered",
    "craftingSpears":          "bookSharpSticks",
    "craftingTraps":           "bookElectricalTraps",
    "craftingVehicles":        "bookVehicleAdventures",
    "craftingWorkstations":    "bookForgeAhead",
}

# Per-skill magazine item name. Used as text_key on the title label so the
# localized magazine name (e.g. "Tech Planet") shows above the cover sprite.
MAG_ITEM: dict[str, str] = {
    "craftingArmor":           "armorSkillMagazine",
    "craftingBlades":          "bladesSkillMagazine",
    "craftingBows":            "bowsSkillMagazine",
    "craftingClubs":           "clubsSkillMagazine",
    "craftingElectrician":     "electricianSkillMagazine",
    "craftingExplosives":      "explosivesSkillMagazine",
    "craftingFood":            "foodSkillMagazine",
    "craftingHandguns":        "handgunsSkillMagazine",
    "craftingHarvestingTools": "harvestingToolsSkillMagazine",
    "craftingKnuckles":        "knucklesSkillMagazine",
    "craftingMachineGuns":     "machineGunsSkillMagazine",
    "craftingMedical":         "medicalSkillMagazine",
    "craftingRepairTools":     "repairToolsSkillMagazine",
    "craftingRifles":          "riflesSkillMagazine",
    "craftingRobotics":        "roboticsSkillMagazine",
    "craftingSalvageTools":    "salvageToolsSkillMagazine",
    "craftingSeeds":           "seedSkillMagazine",
    "craftingShotguns":        "shotgunsSkillMagazine",
    "craftingSledgehammers":   "sledgehammersSkillMagazine",
    "craftingSpears":          "spearsSkillMagazine",
    "craftingTraps":           "trapsSkillMagazine",
    "craftingVehicles":        "vehiclesSkillMagazine",
    "craftingWorkstations":    "workstationSkillMagazine",
}

# Display order for tabs across the top row (file order != display order)
TAB_ORDER = [
    "craftingRobotics", "craftingBows", "craftingHandguns", "craftingRifles",
    "craftingMachineGuns", "craftingShotguns", "craftingHarvestingTools",
    "craftingBlades", "craftingClubs", "craftingKnuckles",
    "craftingSledgehammers", "craftingSpears", "craftingSalvageTools",
    "craftingRepairTools", "craftingArmor", "craftingTraps", "craftingMedical",
    "craftingVehicles", "craftingElectrician", "craftingSeeds",
    "craftingExplosives", "craftingWorkstations", "craftingFood",
]

# Short name (without the ``crafting`` prefix); used in rect names.
def short(skill_name: str) -> str:
    return skill_name[len("crafting"):]

# ---------------------------------------------------------------------------
# Data model
# ---------------------------------------------------------------------------

@dataclass
class DisplayRow:
    """One ``<display_entry>`` from a crafting_skill block.

    Two flavors:
      - tiered (1 item, 6 unlock_level numbers, has quality)
      - non-tiered (N items, M thresholds, has_quality="false")
    """
    items: list[str]                    # item or icon names (flattened)
    unlock_levels: list[int]            # threshold numbers (per quality OR per item)
    name_keys: list[str] = field(default_factory=list)  # tooltip/loc keys
    has_quality: bool = True
    # Non-tiered rows: items grouped by unlock_level position. Each list
    # corresponds to the items unlocked at unlock_levels[i]. For tiered rows
    # this is normally a single group repeated.
    level_items: list[list[str]] = field(default_factory=list)
    level_names: list[list[str]] = field(default_factory=list)
    # Tiered rows only: optional multi-row icon layout. When set, the zoomed
    # qsection becomes 50px taller per extra icon row, with icons stacked at
    # y=-40, y=-90, y=-140, ... Used by Armored Up to show all
    # Light/Medium/Heavy outfit variants in three rows.
    icon_rows: list[list[str]] = field(default_factory=list)
    icon_row_names: list[list[str]] = field(default_factory=list)

    @property
    def is_tiered(self) -> bool:
        # 6 quality thresholds + has_quality=true Ã¢â€ â€™ quality-tiered row
        # (number of items can vary: 1 for typical, 3+ for shared armor tiers)
        return self.has_quality and len(self.unlock_levels) == 6

@dataclass
class Skill:
    name: str            # craftingRifles
    max_level: int
    icon: str
    display: list[DisplayRow] = field(default_factory=list)

    @property
    def short_name(self) -> str:
        return short(self.name)

    @property
    def book_sprite(self) -> str:
        return BOOK_SPRITE.get(self.name, "ui_game_symbol_book")


@dataclass
class BookSeries:
    name: str
    name_key: str
    books: list[str]
    completion_book: str
    completion_desc_key: str

    @property
    def agf_skill_cvar(self) -> str:
        return f"AGF{self.name}"

    @property
    def agf_completion_cvar(self) -> str:
        return f"AGF{self.completion_book}" if self.completion_book else ""


@dataclass
class UnlockEntry:
    name: str
    source: str
    category: str


@dataclass
class UnlockReviewCandidate:
    name: str
    detected_category: str
    detected_subcategory: str
    detected_order: int
    source_tab_key: str
    discovered_from_game_scan: bool = False
    discovered_sources: list[str] = field(default_factory=list)


def parse_book_series(path: Path) -> list[BookSeries]:
    """Parse vanilla book groups and member books from progression.xml."""
    tree = ET.parse(path)
    root = tree.getroot()

    groups = [
        bg for bg in root.iter("book_group")
        if (bg.get("name") or "").startswith("skill")
    ]

    books_by_parent: dict[str, list[ET.Element]] = {}
    for bk in root.iter("book"):
        parent = bk.get("parent", "")
        if parent.startswith("skill"):
            books_by_parent.setdefault(parent, []).append(bk)

    out: list[BookSeries] = []
    for bg in groups:
        name = bg.get("name", "")
        if not name:
            continue
        name_key = bg.get("name_key", "")
        series_books: list[str] = []
        completion_book = ""
        completion_desc = ""
        for bk in books_by_parent.get(name, []):
            perk_name = bk.get("name", "")
            if not perk_name.startswith("perk"):
                continue
            if "Complete" in perk_name:
                completion_book = perk_name
                completion_desc = bk.get("desc_key", "")
                continue
            series_books.append(perk_name)
        if not completion_book and series_books:
            completion_book = series_books[-1]
            series_books = series_books[:-1]
        out.append(BookSeries(
            name=name,
            name_key=name_key,
            books=series_books,
            completion_book=completion_book,
            completion_desc_key=completion_desc,
        ))
    return out


def _validate_books_rect_against_vanilla(rect_xml: str, series_list: list[BookSeries]) -> tuple[bool, list[str]]:
    """Ensure booksTab cvar/tooltip wiring matches vanilla progression series."""
    expected_skills = [bs for bs in series_list if bs.books]
    expected_skill_cvars = {f"AGF{bs.name}" for bs in expected_skills}

    cvars = set(re.findall(r'cvar\(([^)]+)\)', rect_xml))
    tips = set(re.findall(r'tooltip_key="([^"]+)"', rect_xml))
    skill_cvars = {cv for cv in cvars if cv.startswith("AGFskill")}

    problems: list[str] = []

    missing_skills = sorted(expected_skill_cvars - skill_cvars)
    extra_skills = sorted(skill_cvars - expected_skill_cvars)
    if missing_skills:
        problems.append(f"missing skill cvars: {', '.join(missing_skills[:6])}")
    if extra_skills:
        problems.append(f"unexpected skill cvars: {', '.join(extra_skills[:6])}")

    missing_book_cvars: list[str] = []
    missing_book_tips: list[str] = []
    for bs in expected_skills:
        comp_tip = bs.completion_desc_key or bs.name_key
        if bs.completion_book:
            comp = f"AGF{bs.completion_book}"
            if comp not in cvars:
                missing_book_cvars.append(comp)
            not_comp = f"AGFNOT{bs.completion_book}"
            if not_comp not in cvars:
                missing_book_cvars.append(not_comp)
            if comp_tip and comp_tip not in tips:
                missing_book_tips.append(comp_tip)
        for perk_name in bs.books[:7]:
            pcv = f"AGF{perk_name}"
            if pcv not in cvars:
                missing_book_cvars.append(pcv)
            ptip = f"{perk_name}Desc"
            if ptip not in tips:
                missing_book_tips.append(ptip)

    if missing_book_cvars:
        uniq = sorted(set(missing_book_cvars))
        problems.append(f"missing book cvars: {', '.join(uniq[:10])}")
    if missing_book_tips:
        uniq = sorted(set(missing_book_tips))
        problems.append(f"missing book tooltips: {', '.join(uniq[:10])}")

    return len(problems) == 0, problems


UNLOCK_CATEGORY_ORDER = [
    "ToolsWeapons",
    "WorkstationsVehicles",
    "TrapsMedical",
    "Armor",
    "FoodFarming",
    "Ammo",
]

# Unlocks All target geometry (from the validated in-game layout).
# These fixed anchors/row specs are used to preserve the hand-tuned section
# widths and positions when generating unlocks from vanilla data.
UNLOCK_ALL_LAYOUT = {
    "ammo": {
        "label_pos": (40, -102),
        "grid_x": 34,
        "rows": [
            (5, -123), (5, -177), (3, -231), (3, -285),
            (3, -339), (3, -393), (3, -447), (2, -501),
        ],
        "cell": (44, 44),
    },
    "tools": {
        "label_pos": (502, -102),
        "grid_x": 496,
        "rows": [
            (7, -123), (7, -181), (7, -239), (7, -297),
            (7, -355), (7, -413), (7, -515), (7, -573), (7, -631),
        ],
        "row_heights": [1, 1, 1, 1, 1, 2, 1, 1, 1],
        "cell": (44, 44),
    },
    "armors": {
        "label_pos": (844, -102),
        "grid_x": 838,
        "rows": [
            (4, -123), (4, -181), (4, -239), (4, -297),
            (4, -355), (4, -413), (4, -471),
        ],
        "cell": (44, 44),
    },
    "vehicles": {
        "label_pos": (1054, -102),
        "grid_pos": (1048, -123),
        "grid": (2, 7),
        "cell": (44, 44),
    },
    "drone": {
        "label_pos": (1054, -242),
        "grid_pos": (1048, -263),
        "grid": (1, 7),
        "cell": (44, 44),
    },
    "misc": {
        "label_pos": (1054, -338),
        "grid_pos": (1048, -359),
        "grid": (2, 7),
        "cell": (44, 44),
    },
}


def _split_csv(value: str) -> list[str]:
    if not value:
        return []
    return [part.strip() for part in value.split(",") if part.strip()]


def _classify_unlock_category(name: str) -> str:
    n = (name or "").lower()
    if "ammo" in n or "rocket" in n:
        return "Ammo"
    if "armor" in n or "modarmor" in n:
        return "Armor"
    if any(k in n for k in ("food", "drink", "farm", "seed", "crop")):
        return "FoodFarming"
    if any(k in n for k in ("trap", "medical", "bandage", "firstaid", "med")):
        return "TrapsMedical"
    if any(k in n for k in ("vehicle", "workstation", "forge", "chem", "cement", "bench")):
        return "WorkstationsVehicles"
    return "ToolsWeapons"


def _unlock_tab_key_for_category(category: str) -> str:
    return {
        "ToolsWeapons": "agf4UnlocksCategoryToolsWeapons",
        "WorkstationsVehicles": "agf4UnlocksCategoryWorkstationsVehicles",
        "TrapsMedical": "agf4UnlocksCategoryTrapsMedical",
        "Armor": "agf4UnlocksCategoryArmors",
        "FoodFarming": "agf4UnlocksCategoryFoodFarming",
        "Ammo": "agf4UnlocksCategoryAmmo",
    }.get(category, "agf4UnlocksCategoryToolsWeapons")


def _unlock_rect_name(category: str) -> str:
    return {
        "ToolsWeapons": "unlockToolsWeapons",
        "WorkstationsVehicles": "unlockWorkstationsVehicles",
        "TrapsMedical": "unlockTrapsMedical",
        "Armor": "unlockArmors",
        "FoodFarming": "unlockFoodFarming",
        "Ammo": "unlockAmmo",
    }.get(category, "unlockToolsWeapons")


UNLOCK_REVIEW_TOP_CATEGORY_ORDER = [
    "Ammo",
    "Tools & Weapons",
    "Armors",
    "Vehicles",
    "Drones",
    "Misc",
]

UNLOCK_REVIEW_TAB_KEY_BY_TOP_CATEGORY = {
    "Ammo": "agf4UnlocksCategoryAmmo",
    "Tools & Weapons": "agf4UnlocksCategoryToolsWeapons",
    "Armors": "agf4UnlocksCategoryArmors",
    "Vehicles": "agf4UnlocksCategoryVehicles",
    "Drones": "agf4UnlocksCategoryDrone",
    "Misc": "agf4UnlocksCategoryMisc",
}

UNLOCK_REVIEW_TOP_CATEGORY_BY_TAB_KEY = {
    "agf4UnlocksCategoryAmmo": "Ammo",
    "agf4UnlocksCategoryToolsWeapons": "Tools & Weapons",
    "agf4UnlocksCategoryArmors": "Armors",
    "agf4UnlocksCategoryVehicles": "Vehicles",
    "agf4UnlocksCategoryDrone": "Drones",
    "agf4UnlocksCategoryMisc": "Misc",
    "agfUnlockAmmo": "Ammo",
    "agfUnlockToolsWeapons": "Tools & Weapons",
    "agfUnlockArmors": "Armors",
    "agfUnlockVehicles": "Vehicles",
    "agfUnlockDrone": "Drones",
    "agfUnlockMisc": "Misc",
}

UNLOCK_REVIEW_SUBCATEGORY_BY_TEXT_KEY = {
    "agf4UnlocksCategoryAmmoRow44": ".44 Ammo",
    "agf4UnlocksCategoryAmmoRow762": "7.62 Ammo",
    "agf4UnlocksCategoryAmmoRow9mm": "9mm Ammo",
    "agf4UnlocksCategoryAmmoRowArrows": "Arrows",
    "agf4UnlocksCategoryAmmoRowBolts": "Bolts",
    "agf4UnlocksCategoryAmmoRowJunkTurret": "Junk Turret Ammo",
    "agf4UnlocksCategoryAmmoRowRockets": "Rockets",
    "agf4UnlocksCategoryAmmoRowShotgun": "Shotgun Ammo",
    "agf4UnlocksCategoryArmorsBoots": "Boots",
    "agf4UnlocksCategoryArmorsFittings": "Fittings",
    "agf4UnlocksCategoryArmorsGeneral": "General",
    "agf4UnlocksCategoryArmorsHelmet": "Helmets",
    "agf4UnlocksCategoryArmorsMuffledConnectors": "Muffled Connectors",
    "agf4UnlocksCategoryArmorsPlating": "Platings",
    "agf4UnlocksCategoryArmorsPockets": "Pockets",
    "agf4UnlocksCategoryToolsWeaponsBarrels": "Barrels",
    "agf4UnlocksCategoryToolsWeaponsBlockDamage": "Block Damage",
    "agf4UnlocksCategoryToolsWeaponsClub": "Clubs",
    "agf4UnlocksCategoryToolsWeaponsMotorTool": "Motor Tools",
    "agf4UnlocksCategoryToolsWeaponsOtherGun": "Ranged General",
    "agf4UnlocksCategoryToolsWeaponsOtherMelee": "Melee General",
    "agf4UnlocksCategoryToolsWeaponsScopes": "Scopes",
    "agf4UnlocksCategoryToolsWeaponsShotgun": "Shotguns",
    "agf4UnlocksCategoryToolsWeaponsSpecial": "Special",
    "agfUnlockAmmoRow44": ".44 Ammo",
    "agfUnlockAmmoRow762": "7.62 Ammo",
    "agfUnlockAmmoRow9mm": "9mm Ammo",
    "agfUnlockAmmoRowArrows": "Arrows",
    "agfUnlockAmmoRowBolts": "Bolts",
    "agfUnlockAmmoRowJunkTurret": "Junk Turret Ammo",
    "agfUnlockAmmoRowRockets": "Rockets",
    "agfUnlockAmmoRowShotgun": "Shotgun Ammo",
    "agfUnlockArmorsBoots": "Boots",
    "agfUnlockArmorsFittings": "Fittings",
    "agfUnlockArmorsGeneral": "General",
    "agfUnlockArmorsHelmet": "Helmets",
    "agfUnlockArmorsMuffledConnectors": "Muffled Connectors",
    "agfUnlockArmorsPlating": "Platings",
    "agfUnlockArmorsPockets": "Pockets",
    "agfUnlockToolsWeaponsBarrels": "Barrels",
    "agfUnlockToolsWeaponsBlockDamage": "Block Damage",
    "agfUnlockToolsWeaponsClub": "Clubs",
    "agfUnlockToolsWeaponsMotorTool": "Motor Tools",
    "agfUnlockToolsWeaponsOtherGun": "Ranged General",
    "agfUnlockToolsWeaponsOtherMelee": "Melee General",
    "agfUnlockToolsWeaponsScopes": "Scopes",
    "agfUnlockToolsWeaponsShotgun": "Shotguns",
    "agfUnlockToolsWeaponsSpecial": "Special",
}

UNLOCK_REVIEW_SUBCATEGORY_OPTIONS = {
    "Ammo": [
        "Arrows",
        "Bolts",
        "9mm Ammo",
        "7.62 Ammo",
        ".44 Ammo",
        "Shotgun Ammo",
        "Junk Turret Ammo",
        "Rockets",
    ],
    "Tools & Weapons": [
        "Melee General",
        "Block Damage",
        "Clubs",
        "Motor Tools",
        "Special",
        "Ranged General",
        "Scopes",
        "Barrels",
        "Shotguns",
    ],
    "Armors": [
        "General",
        "Helmets",
        "Boots",
        "Fittings",
        "Muffled Connectors",
        "Pockets",
        "Platings",
    ],
    "Vehicles": [],
    "Drones": [],
    "Misc": [],
}


def _normalize_unlock_name_key(name: str) -> str:
    return (name or "").strip().lower()


def _unlock_review_tab_key_for_category(category: str) -> str:
    return UNLOCK_REVIEW_TAB_KEY_BY_TOP_CATEGORY.get(category, "")


def _is_unlock_review_game_candidate(name: str) -> bool:
    nm = _normalize_unlock_name_key(name)
    if not nm:
        return False
    if nm.startswith("ammobundle"):
        return False

    prefixes = (
        "mod",
        "ammo",
        "vehicle",
        "tool",
        "resource",
        "food",
        "drink",
        "planted",
        "drug",
        "medical",
        "cnt",
        "forge",
        "workbench",
        "cementmixer",
        "chemistrystation",
        "generatorbank",
        "batterybank",
        "switch",
        "tripwire",
        "motionsensor",
        "electrictimerrelay",
        "pressureplate",
        "spotlight",
        "speaker",
        "electricfencepost",
        "darttrap",
        "bladetrap",
        "autoturret",
        "shotgunturret",
        "m60turret",
        "vault",
        "irongaragedoor",
        "woodengaragedoor",
        "steelgaragedoor",
        "metalreinforcedwooddrawbridge",
    )
    return nm.startswith(prefixes)


def _infer_unlock_review_category_from_name(name: str) -> str:
    nm = _normalize_unlock_name_key(name)
    if not nm:
        return "Misc"
    if "drone" in nm:
        return "Drones"
    if nm.startswith("vehicle") or nm.startswith("modvehicle") or "gascan" in nm:
        return "Vehicles"
    if nm.startswith("ammo") or "rocket" in nm:
        return "Ammo"
    if nm.startswith("modarmor"):
        return "Armors"

    misc_prefixes = (
        "food",
        "drink",
        "planted",
        "drug",
        "medical",
        "resource",
        "cnt",
        "forge",
        "workbench",
        "cementmixer",
        "chemistrystation",
        "generatorbank",
        "batterybank",
        "switch",
        "tripwire",
        "motionsensor",
        "electrictimerrelay",
        "pressureplate",
        "spotlight",
        "speaker",
        "electricfencepost",
        "darttrap",
        "bladetrap",
        "autoturret",
        "shotgunturret",
        "m60turret",
        "vault",
        "irongaragedoor",
        "woodengaragedoor",
        "steelgaragedoor",
        "metalreinforcedwooddrawbridge",
        "tool",
    )
    if nm.startswith(misc_prefixes):
        return "Misc"
    return "Tools & Weapons"


def _infer_unlock_review_subcategory_from_name(category: str, name: str) -> str:
    nm = _normalize_unlock_name_key(name)
    if category == "Ammo":
        if "arrow" in nm:
            return "Arrows"
        if "bolt" in nm:
            return "Bolts"
        if "9mm" in nm:
            return "9mm Ammo"
        if "762" in nm or "7.62" in nm:
            return "7.62 Ammo"
        if "44" in nm:
            return ".44 Ammo"
        if "shotgun" in nm:
            return "Shotgun Ammo"
        if "junkturret" in nm:
            return "Junk Turret Ammo"
        if "rocket" in nm:
            return "Rockets"
        return ""

    if category == "Tools & Weapons":
        if "scope" in nm:
            return "Scopes"
        if any(token in nm for token in ("barrel", "muzzle", "choke", "sawedoff")):
            return "Barrels"
        if "club" in nm:
            return "Clubs"
        if any(token in nm for token in ("motor", "chainsaw", "auger")):
            return "Motor Tools"
        if "shotgun" in nm:
            return "Shotguns"
        if any(token in nm for token in ("block", "woodsplitter", "firemansaxe", "gravedigger", "ironbreaker", "bunkerbuster")):
            return "Block Damage"
        if nm.startswith("modmelee") or "melee" in nm:
            return "Melee General"
        if nm.startswith("modgun") or nm.startswith("gun"):
            return "Ranged General"
        if nm.startswith("mod"):
            return "Special"
        return ""

    if category == "Armors":
        if "helmet" in nm:
            return "Helmets"
        if "boot" in nm:
            return "Boots"
        if "fitting" in nm:
            return "Fittings"
        if "muffled" in nm:
            return "Muffled Connectors"
        if "pocket" in nm:
            return "Pockets"
        if "plating" in nm:
            return "Platings"
        if nm.startswith("modarmor"):
            return "General"
        return ""

    return ""

# ---------------------------------------------------------------------------
# Parsing
# ---------------------------------------------------------------------------

@dataclass
class ItemIcon:
    """Resolved icon properties for a single item from items.xml.

    sprite : sprite name in ItemIconAtlas (defaults to the item name).
    tint   : NGUI tint as "R,G,B" (e.g. "160,160,255") or "" if none.
    type_icon : overlay sprite (CustomIconTint's challenge marker etc.) or "".
    """
    sprite: str
    tint: str = ""
    type_icon: str = ""


def _hex_to_rgb(hexstr: str) -> str:
    s = hexstr.strip().lstrip("#")
    if len(s) != 6:
        return ""
    try:
        r = int(s[0:2], 16); g = int(s[2:4], 16); b = int(s[4:6], 16)
    except ValueError:
        return ""
    return f"{r},{g},{b}"


def parse_items(path: Path, tag: str = "item") -> dict[str, ItemIcon]:
    """Read every ``<item name="...">`` (or ``<block>`` when ``tag="block"``)
    in the file and capture icon-related properties: CustomIcon,
    CustomIconTint, ItemTypeIcon. Inheritance via ``Extends`` IS followed and
    ``param1="prop1,prop2"`` lists EXCLUDED properties (so a child won't
    inherit those from its parent)."""
    out: dict[str, ItemIcon] = {}
    try:
        tree = ET.parse(path)
    except (FileNotFoundError, ET.ParseError):
        return out

    root = tree.getroot()
    raw: dict[str, dict[str, str]] = {}
    for node in root.iter(tag):
        nm = node.get("name", "")
        if not nm:
            continue
        d: dict[str, str] = {}
        ext = node.get("Extends") or ""
        # Extends param1 lists excluded inherited properties.
        ext_excl: list[str] = []
        for prop in node.findall("property"):
            pname = prop.get("name", "")
            pval = prop.get("value", "")
            if pname == "Extends":
                ext = pval
                p1 = prop.get("param1", "")
                if p1:
                    ext_excl = [s.strip() for s in p1.split(",") if s.strip()]
            elif pname in ("CustomIcon", "CustomIconTint", "ItemTypeIcon", "SelectAlternates", "PlaceAltBlockValue"):
                d[pname] = pval
        if ext:
            d["__ext__"] = ext
        if ext_excl:
            d["__excl__"] = ",".join(ext_excl)
        raw[nm] = d

    def resolve(nm: str, depth: int = 0) -> dict[str, str]:
        if depth > 16 or nm not in raw:
            return {}
        own = raw[nm]
        parent_name = own.get("__ext__", "")
        excl = set((own.get("__excl__") or "").split(",")) if own.get("__excl__") else set()
        merged: dict[str, str] = {}
        if parent_name:
            inherited = resolve(parent_name, depth + 1)
            for k, v in inherited.items():
                if k in excl:
                    continue
                merged[k] = v
        for k, v in own.items():
            if k not in ("__ext__", "__excl__"):
                merged[k] = v
        return merged

    for nm in raw:
        m = resolve(nm)
        sprite = m.get("CustomIcon", "") or nm
        tint_hex = m.get("CustomIconTint", "")
        tint = _hex_to_rgb(tint_hex) if tint_hex else ""

        type_icon_name = (m.get("ItemTypeIcon", "") or "").strip()

        # Variant helpers are selector wrappers; use the first real alternate's
        # type icon so display overlays match what players actually place.
        if nm.endswith("BlockVariantHelper") and (m.get("SelectAlternates", "").lower() == "true"):
            alt_values = [s.strip() for s in (m.get("PlaceAltBlockValue", "") or "").split(",") if s.strip()]
            if alt_values:
                alt_meta = resolve(alt_values[0])
                alt_type_icon = (alt_meta.get("ItemTypeIcon", "") or "").strip()
                if alt_type_icon:
                    type_icon_name = alt_type_icon
        # Cosmetic Lock Icon compatibility: ignore the scrap marker overlay so
        # it never appears in Purple Book generated output.
        type_icon_norm = type_icon_name.lower()
        if type_icon_norm in ("scrap", "ui_game_symbol_scrap"):
            type_icon_name = ""

        if not type_icon_name:
            type_icon_sprite = ""
        elif type_icon_name.startswith("ui_"):
            type_icon_sprite = type_icon_name
        elif type_icon_name == "bundle":
            type_icon_sprite = "ui_game_symbol_treasure"
        else:
            type_icon_sprite = f"ui_game_symbol_{type_icon_name}"

        out[nm] = ItemIcon(sprite=sprite, tint=tint, type_icon=type_icon_sprite)

    return out


def _parse_first_last_numeric(csv_values: str) -> tuple[float, float] | None:
    vals = [s.strip() for s in csv_values.split(",") if s.strip()]
    if not vals:
        return None
    try:
        nums = [float(v) for v in vals]
    except ValueError:
        return None
    return nums[0], nums[-1]


def _parse_numeric_series(csv_values: str) -> list[float] | None:
    vals = [s.strip() for s in (csv_values or "").split(",") if s.strip()]
    if not vals:
        return None
    try:
        nums = [float(v) for v in vals]
    except ValueError:
        return None
    return nums


def _format_percent_value(v: float) -> str:
    # items.xml mixes normalized (0.02) and percent (2) notations.
    p = v * 100.0 if abs(v) <= 1.0 else v
    if abs(p - round(p)) < 1e-9:
        return f"{int(round(p))}%"
    return f"{p:.1f}".rstrip("0").rstrip(".") + "%"


def _format_plain_value(v: float) -> str:
    if abs(v - round(v)) < 1e-9:
        return f"{int(round(v))}"
    return f"{v:.1f}".rstrip("0").rstrip(".")


def parse_armor_value_series_by_item(path: Path) -> dict[str, list[list[float]]]:
    """Read candidate numeric tier-series for armor piece items from items.xml."""
    out: dict[str, list[list[float]]] = {}
    try:
        tree = ET.parse(path)
    except (FileNotFoundError, ET.ParseError):
        return out

    root = tree.getroot()
    for item in root.findall(".//item"):
        item_name = (item.get("name") or "").strip()
        if not item_name.startswith("armor"):
            continue

        if not (
            item_name.endswith("Helmet")
            or item_name.endswith("Outfit")
            or item_name.endswith("Gloves")
            or item_name.endswith("Boots")
        ):
            continue

        candidates: list[list[float]] = []
        for node in item.findall(".//passive_effect") + item.findall(".//display_value") + item.findall(".//triggered_effect"):
            series = _parse_numeric_series(node.get("value", ""))
            if series is None:
                continue
            if len(series) < 2:
                continue
            if len(series) not in (2, 6):
                continue
            candidates.append(series)

        if not candidates:
            continue

        deduped: list[list[float]] = []
        seen: set[tuple[float, ...]] = set()
        for seq in candidates:
            key = tuple(seq)
            if key in seen:
                continue
            seen.add(key)
            deduped.append(seq)

        out[item_name] = deduped

    return out


def _parse_display_number(text: str) -> tuple[float, bool] | None:
    t = (text or "").strip()
    if not t:
        return None
    is_percent = t.endswith("%")
    if is_percent:
        t = t[:-1].strip()
    try:
        return float(t), is_percent
    except ValueError:
        return None


def _format_value_like(v: float, as_percent: bool) -> str:
    if as_percent:
        return _format_percent_value(v)
    return _format_plain_value(v)


def _resolve_armor_row_values_for_display(
    item_name: str,
    q1_text: str,
    q6_text: str,
) -> list[str] | None:
    parsed_q1 = _parse_display_number(q1_text)
    parsed_q6 = _parse_display_number(q6_text)
    if parsed_q1 is None or parsed_q6 is None:
        return None

    q1_num, q1_percent = parsed_q1
    q6_num, q6_percent = parsed_q6
    if q1_percent != q6_percent:
        return None

    candidates = ARMOR_VALUE_SERIES_BY_ITEM.get(item_name, [])
    best: list[float] | None = None
    best_score = float("inf")

    for seq in candidates:
        if len(seq) != 6:
            continue
        first = _parse_display_number(_format_value_like(seq[0], q1_percent))
        last = _parse_display_number(_format_value_like(seq[-1], q1_percent))
        if first is None or last is None:
            continue

        first_num, _ = first
        last_num, _ = last
        score = abs(first_num - q1_num) + abs(last_num - q6_num)
        if score < best_score:
            best_score = score
            best = seq

    if best is None:
        return None

    return [_format_value_like(v, q1_percent) for v in best]


def _armor_piece_item_name_for_row(grid_name: str, row: int) -> str | None:
    if not (grid_name.startswith("armor") and grid_name.endswith("Outfit")):
        return None
    base = grid_name[len("armor") : -len("Outfit")]
    suffix_by_row = {2: "Helmet", 3: "Outfit", 4: "Gloves", 5: "Boots"}
    suffix = suffix_by_row.get(row)
    if suffix is None:
        return None
    return f"armor{base}{suffix}"


def parse_armor_setbonus_numeric_series_from_buffs(path: Path) -> dict[str, dict[int, list[float]]]:
    """Read set-bonus q1..q6 numeric series by outfit row from vanilla buffs.xml.

    Output shape:
    - key: outfit grid name (e.g. armorAssassinOutfit)
    - value: {6: [q1..q6], 7: [q1..q6]} where row 6 maps to FSBDisplay/FSBDisplay01
      and row 7 maps to FSBDisplay02 when present.
    """
    out: dict[str, dict[int, list[float]]] = {}
    try:
        tree = ET.parse(path)
    except (FileNotFoundError, ET.ParseError):
        return out

    root = tree.getroot()
    for buff in root.findall(".//buff"):
        buff_name = (buff.get("name") or "").strip()
        m = re.match(r"^buff([A-Za-z0-9_]+)SetBonus$", buff_name)
        if not m:
            continue
        armor_name = m.group(1)
        outfit_name = f"armor{armor_name}Outfit"

        by_row_q: dict[int, dict[int, float]] = {6: {}, 7: {}}

        for eg in buff.findall(".//effect_group"):
            q: int | None = None
            for req in eg.findall(".//requirement"):
                if req.get("name") != "ArmorGroupLowestQuality" or req.get("operation") != "Equals":
                    continue
                try:
                    q = int((req.get("value") or "").strip())
                except ValueError:
                    q = None
                break

            if q is None or q < 1 or q > 6:
                continue

            for te in eg.findall("triggered_effect"):
                if te.get("operation") != "set":
                    continue
                cvar = (te.get("cvar") or "").strip()
                if "FSBDisplay" not in cvar:
                    continue
                try:
                    val = float((te.get("value") or "").strip())
                except ValueError:
                    continue

                if cvar.endswith("FSBDisplay02"):
                    row = 7
                else:
                    row = 6

                by_row_q[row][q] = val

        row_series: dict[int, list[float]] = {}
        for row in (6, 7):
            q_map = by_row_q[row]
            if all(q in q_map for q in range(1, 7)):
                row_series[row] = [q_map[q] for q in range(1, 7)]

        if row_series:
            out[outfit_name] = row_series

    return out


def _resolve_setbonus_row_values_for_display(
    grid_name: str,
    row: int,
    q1_text: str,
    q6_text: str,
) -> list[str] | None:
    # Manual/split-stat rows are always authoritative and never interpolated.
    manual_rows = ARMOR_SETBONUS_ROW_SERIES_OVERRIDES.get(grid_name, {})
    if row in manual_rows:
        vals = manual_rows[row]
        if len(vals) == 6:
            return vals

    parsed_q1 = _parse_display_number(q1_text)
    parsed_q6 = _parse_display_number(q6_text)
    if parsed_q1 is None or parsed_q6 is None:
        q1 = (q1_text or "").strip()
        q6 = (q6_text or "").strip()
        if q1 and q1 == q6:
            return [q1, q1, q1, q1, q1, q1]
        return None

    q1_num, q1_percent = parsed_q1
    q6_num, q6_percent = parsed_q6
    if q1_percent != q6_percent:
        return None

    raw_rows = ARMOR_SETBONUS_NUMERIC_SERIES_BY_OUTFIT.get(grid_name, {})
    raw_series = raw_rows.get(row)
    if raw_series is None or len(raw_series) != 6:
        q1 = (q1_text or "").strip()
        q6 = (q6_text or "").strip()
        if q1 and q1 == q6:
            return [q1, q1, q1, q1, q1, q1]
        return None

    raw_q1 = raw_series[0]
    raw_q6 = raw_series[-1]
    scale_candidates: list[float] = []
    if abs(raw_q1) > 1e-9:
        scale_candidates.append(q1_num / raw_q1)
    if abs(raw_q6) > 1e-9:
        scale_candidates.append(q6_num / raw_q6)
    if not scale_candidates:
        return None

    scale = scale_candidates[-1]
    if len(scale_candidates) == 2 and abs(scale_candidates[0] - scale_candidates[1]) <= 1e-6:
        scale = 0.5 * (scale_candidates[0] + scale_candidates[1])

    return [_format_value_like(v * scale, q1_percent) for v in raw_series]


def _csv_row_to_line_language_quoted(row: list[str], language_indices: set[int]) -> str:
    """Build a CSV line where only selected language columns are double-quoted."""
    cols: list[str] = []
    for idx, val in enumerate(row):
        text = val or ""
        if idx in language_indices:
            cols.append('"' + text.replace('"', '""') + '"')
        else:
            if any(ch in text for ch in [",", '"', "\n", "\r"]):
                cols.append('"' + text.replace('"', '""') + '"')
            else:
                cols.append(text)
    return ",".join(cols)


def _read_localization_header(path: Path) -> list[str]:
    try:
        with path.open("r", encoding="utf-8", newline="") as f:
            reader = csv.reader(f)
            header = next(reader, None)
            if header:
                return header
    except (FileNotFoundError, OSError, UnicodeError, csv.Error):
        pass
    return []


def _resolve_game_localization_path(path: Path) -> Path:
    if path.exists():
        return path
    txt_alt = path.with_suffix(".txt")
    csv_alt = path.with_suffix(".csv")
    if txt_alt.exists():
        return txt_alt
    if csv_alt.exists():
        return csv_alt
    return path


def _localization_schema(header_fields: list[str]) -> tuple[int, dict[str, int], list[str]]:
    if not header_fields:
        header_fields = DEFAULT_LOCALIZATION_HEADER_FIELDS[:]

    english_index = 5
    language_indices: dict[str, int] = {}
    for idx, col in enumerate(header_fields):
        name = (col or "").strip().lower()
        if name == "english":
            english_index = idx
        if name in LOCALIZATION_LANGUAGE_HEADERS and name != "english":
            language_indices[name] = idx

    return english_index, language_indices, header_fields


def _row_to_csv_line(row: list[str]) -> str:
    buf = io.StringIO()
    writer = csv.writer(buf, lineterminator="")
    writer.writerow(row)
    return buf.getvalue()


def _extract_set_bonus_paragraph(localized_text: str) -> str:
    text = (localized_text or "")
    text = text.replace("\\r\\n", "\n").replace("\\n", "\n")
    text = text.replace("\r\n", "\n").strip()
    if not text:
        return ""
    parts = [p.strip() for p in re.split(r"\n\s*\n", text) if p.strip()]
    if len(parts) >= 2:
        return parts[-1]
    # Primitive (and any unexpected rows) can lack a dedicated full-set line.
    return ""


def _extract_piece_description_without_set_bonus(localized_text: str) -> str:
    """Return armor piece description text without trailing full-set paragraph."""
    text = (localized_text or "")
    text = text.replace("\\r\\n", "\n").replace("\\n", "\n")
    text = text.replace("\r\n", "\n").strip()
    if not text:
        return ""
    parts = [p.strip() for p in re.split(r"\n\s*\n", text) if p.strip()]
    if len(parts) >= 2:
        # Vanilla armor piece descriptions keep the full-set bonus in the
        # final paragraph; keep only the description section(s).
        core = "\n\n".join(parts[:-1]).strip()
    else:
        core = text

    # Remove the leading armor-type heading line (e.g. "Light Armor") so the
    # output contains only the per-piece description body.
    lines = [ln for ln in core.split("\n")]
    if len(lines) >= 2:
        return "\n".join(lines[1:]).strip()
    return core.strip()


def _force_two_line_piece_description(text: str) -> str:
    """Format piece description into at most two lines for zoom panel labels.

    Wrapping support in 7DTD label widgets is inconsistent across contexts, so
    we insert explicit newlines when needed.
    """
    t = (text or "").strip()
    if not t:
        return ""
    if "\n" in t:
        return t

    # Keep split logic tied to the rendered description column geometry.
    # This targets the merged description column used in zoom armor rows.
    target_width_px = ZOOM_ARMOR_DESC_LABEL_WIDTH
    target_font_size = ZOOM_ARMOR_DESC_FONT_SIZE
    contains_cjk = bool(re.search(r"[\u3040-\u30ff\u3400-\u4dbf\u4e00-\u9fff\uf900-\ufaff]", t))
    avg_char_px = target_font_size * (1.0 if contains_cjk else 0.35)
    max_chars_per_line = max(18, int(target_width_px // max(1.0, avg_char_px)))

    if len(t) <= max_chars_per_line:
        return t

    split_at = -1
    if " " in t:
        # Greedy fit: push the first line as far right as possible to avoid
        # wasting horizontal space in the description box.
        target = min(len(t) - 1, max_chars_per_line + 3)
        spaces = [m.start() for m in re.finditer(r"\s+", t) if 0 < m.start() < len(t) - 1]
        min_right_chars = max(10, int(max_chars_per_line * 0.25))
        before = [i for i in spaces if i <= target and len(t[i + 1 :].lstrip()) >= min_right_chars]
        if before:
            split_at = max(before)
        else:
            # If greedy cut would orphan a tiny second line, move left just
            # enough to give line 2 useful content.
            salvage = [i for i in spaces if len(t[i + 1 :].lstrip()) >= min_right_chars]
            if salvage:
                split_at = max(salvage)
            else:
                after = [i for i in spaces if i > target]
                if after:
                    split_at = min(after)
    else:
        # No natural space break (CJK or compact text): split near balanced fit.
        split_at = min(len(t) - 1, max_chars_per_line + 2)

    if split_at > 0:
        left = t[:split_at].rstrip()
        right = t[split_at + 1 :].lstrip() if " " in t else t[split_at:].lstrip()
        if left and right:
            return f"{left}\\n{right}"

    return t


def parse_armor_setbonus_desc_rows_from_game_localization(path: Path) -> dict[str, str]:
    """Build full multilingual CSV rows for armorNAMESetBonusDesc from vanilla localization."""
    out: dict[str, str] = {}
    try:
        with path.open("r", encoding="utf-8", newline="") as f:
            reader = csv.reader(f)
            header = next(reader, None)
            if not header:
                return out
            ncols = len(header)
            language_indices = {
                idx
                for idx, col in enumerate(header)
                if (col or "").strip().lower() in LOCALIZATION_LANGUAGE_HEADERS
            }
            english_idx = next(
                (idx for idx, col in enumerate(header) if (col or "").strip().lower() == "english"),
                None,
            )

            # Some armor set-bonus text in armorOutfitDesc omits secondary
            # effects in v3.0; keep generated English copy aligned to gameplay.
            setbonus_desc_english_overrides = {
                "Lumberjack": "Full Set Bonus: Double wood harvest with axes and chainsaws, and use less stamina when using axes.",
                "Preacher": "Full Set Bonus: Reduces the chance of critical injuries and infection.",
            }
            for row in reader:
                if len(row) < 1:
                    continue
                key = (row[0] or "").strip()
                m = re.match(r"^armor([A-Za-z0-9_]+)OutfitDesc$", key)
                if not m:
                    continue
                armor_name = m.group(1)
                target_key = f"agfArmor{armor_name}SetBonusDesc"

                row_out = [""] * ncols
                row_out[0] = target_key
                # Keep non-language metadata columns aligned with source row.
                for idx in range(1, min(len(row), ncols)):
                    if idx not in language_indices:
                        row_out[idx] = row[idx]

                # Copy true language columns with only the set-bonus paragraph.
                for idx in sorted(language_indices):
                    src = row[idx] if idx < len(row) else ""
                    bonus_paragraph = _extract_set_bonus_paragraph(src)
                    row_out[idx] = bonus_paragraph

                # Primitive has no vanilla set-bonus paragraph; provide explicit multilingual text.
                if not any((row_out[idx] or "").strip() for idx in language_indices):
                    # Keep literals ASCII-safe in source to prevent mojibake regressions
                    # from editor/encoding drift when this file is modified.
                    primitive_fallback = {
                        "english": "Full Set Bonus: No set bonus.",
                        "german": "Vollst\u00e4ndiger Setbonus: Kein Setbonus.",
                        "spanish": "Bono de conjunto completo: Sin bonificaci\u00f3n de conjunto.",
                        "french": "Bonus d'ensemble complet : aucun bonus d'ensemble.",
                        "italian": "Bonus set completo: nessun bonus set.",
                        "japanese": "\u30d5\u30eb\u30bb\u30c3\u30c8\u30dc\u30fc\u30ca\u30b9\uff1a \u30bb\u30c3\u30c8\u30dc\u30fc\u30ca\u30b9\u306a\u3057\u3002",
                        "koreana": "\uc138\ud2b8 \uc644\uc131 \ubcf4\ub108\uc2a4: \uc138\ud2b8 \ubcf4\ub108\uc2a4 \uc5c6\uc74c.",
                        "polish": "Premia za pe\u0142ny zestaw: brak premii za zestaw.",
                        "brazilian": "B\u00f4nus de conjunto completo: sem b\u00f4nus de conjunto.",
                        "russian": "\u0411\u043e\u043d\u0443\u0441 \u0437\u0430 \u043f\u043e\u043b\u043d\u044b\u0439 \u043a\u043e\u043c\u043f\u043b\u0435\u043a\u0442: \u0431\u043e\u043d\u0443\u0441 \u0437\u0430 \u043a\u043e\u043c\u043f\u043b\u0435\u043a\u0442 \u043e\u0442\u0441\u0443\u0442\u0441\u0442\u0432\u0443\u0435\u0442.",
                        "turkish": "Tam Set Bonusu: Set bonusu yok.",
                        "schinese": "\u5168\u5957\u5956\u52b1\uff1a\u65e0\u5957\u88c5\u5956\u52b1\u3002",
                        "tchinese": "\u5957\u88dd\u734e\u52f5\uff1a\u7121\u5957\u88dd\u734e\u52f5\u3002",
                    }
                    for idx in sorted(language_indices):
                        col_name = (header[idx] if idx < len(header) else "").strip().lower()
                        row_out[idx] = primitive_fallback.get(col_name, primitive_fallback["english"])

                if english_idx is not None:
                    override_english = setbonus_desc_english_overrides.get(armor_name)
                    if override_english:
                        row_out[english_idx] = override_english

                out[target_key] = _csv_row_to_line_language_quoted(row_out, language_indices)
    except (FileNotFoundError, OSError, UnicodeError, csv.Error):
        return out
    return out


def parse_armor_piece_desc_rows_from_game_localization(path: Path) -> dict[str, str]:
    """Build multilingual rows for custom agf armor piece description keys.

    Source rows are vanilla armor*<Piece>Desc entries where the last paragraph
    is a full-set bonus note. That trailing paragraph is removed.
    """
    out: dict[str, str] = {}
    try:
        with path.open("r", encoding="utf-8", newline="") as f:
            reader = csv.reader(f)
            header = next(reader, None)
            if not header:
                return out
            ncols = len(header)
            language_indices = {
                idx
                for idx, col in enumerate(header)
                if (col or "").strip().lower() in LOCALIZATION_LANGUAGE_HEADERS
            }
            part_suffix_map = {
                "Helmet": "Helmet",
                "Outfit": "Outfit",
                "Gloves": "Glove",
                "Boots": "Boots",
            }
            for row in reader:
                if len(row) < 1:
                    continue
                key = (row[0] or "").strip()
                m = re.match(r"^armor([A-Za-z0-9_]+)(Helmet|Outfit|Gloves|Boots)Desc$", key)
                if not m:
                    continue
                armor_name = m.group(1)
                piece_suffix = part_suffix_map.get(m.group(2), m.group(2))
                target_key = f"agfArmor{armor_name}{piece_suffix}Description"

                row_out = [""] * ncols
                row_out[0] = target_key
                # Keep non-language metadata columns aligned with source row.
                for idx in range(1, min(len(row), ncols)):
                    if idx not in language_indices:
                        row_out[idx] = row[idx]

                for idx in sorted(language_indices):
                    src = row[idx] if idx < len(row) else ""
                    piece_text = _extract_piece_description_without_set_bonus(src)
                    # Native UI wrapping test mode: remove hard line breaks and
                    # keep one physical CSV line per key.
                    piece_text = re.sub(r"\s+", " ", piece_text.replace("\r\n", "\n").replace("\n", " ")).strip()
                    row_out[idx] = piece_text

                if any((row_out[idx] or "").strip() for idx in language_indices):
                    out[target_key] = _csv_row_to_line_language_quoted(row_out, language_indices)
    except (FileNotFoundError, OSError, UnicodeError, csv.Error):
        return out
    return out


def _is_generated_armor_piece_desc_key(key: str) -> bool:
    return bool(re.match(r"^agf[A-Za-z0-9_]+(Helmet|Outfit|Glove|Boots)Description$", key or ""))


def _derive_setbonus_parts_from_desc_english(desc_text: str) -> tuple[str, str]:
    """Derive SetBonus1/2 from an English full-set description sentence."""
    text = (desc_text or "").strip()
    if not text:
        return "none", "none"

    text = re.sub(r"^\s*Full\s+Set\s+Bonus\s*:\s*", "", text, flags=re.IGNORECASE).strip()
    text = re.sub(r"\s+", " ", text)
    if not text:
        return "none", "none"

    lowered = text.lower().strip(" .")
    if lowered in ("no set bonus", "none"):
        return "No Set Bonus", "none"

    # Prefer sentence boundaries first.
    sentences = [s.strip(" .") for s in re.split(r"(?<=[.!?])\s+", text) if s.strip()]
    if len(sentences) >= 2:
        return sentences[0], sentences[1]

    one = sentences[0] if sentences else text.strip(" .")

    # Fallback: split one sentence into two bonuses only when both halves
    # appear to describe an action/effect.
    verb_hints = (
        "is ", "are ", "use", "uses", "using", "does", "do ",
        "reload", "reloading", "find", "gain", "gains", "reduce",
        "reduces", "recover", "recovery", "degrade", "degrades",
        "work", "works", "faster", "slower", "increased", "increases",
        "improved", "improves",
    )

    def _has_effect_clause(s: str) -> bool:
        t = f" {s.lower()} "
        return any(h in t for h in verb_hints)

    and_parts = re.split(r"\s+and\s+", one, maxsplit=1, flags=re.IGNORECASE)
    if len(and_parts) == 2:
        left = and_parts[0].strip(" .,")
        right = and_parts[1].strip(" .,")
        if left and right and _has_effect_clause(left) and _has_effect_clause(right):
            return left, right

    return one, "none"


def derive_setbonus_rows_from_desc_rows(
    armor_setbonus_desc_rows: dict[str, str],
    english_index: int = 5,
) -> dict[str, str]:
    """Build armorNAMESetBonus1/2 map by parsing english from SetBonusDesc rows."""
    out: dict[str, str] = {}
    for key, line in armor_setbonus_desc_rows.items():
        m = re.match(r"^agfArmor([A-Za-z0-9_]+)SetBonusDesc$", key)
        if not m:
            continue
        armor_name = m.group(1)
        try:
            parsed = next(csv.reader([line]))
        except csv.Error:
            continue
        english = parsed[english_index].strip() if len(parsed) > english_index else ""
        b1, b2 = _derive_setbonus_parts_from_desc_english(english)

        # Keep short UI phrasing explicit where vanilla effect naming is clearer.
        if armor_name.lower() == "assassin":
            b1, b2 = "Enemy Search Time", "none"
        elif armor_name.lower() == "athletic":
            b1, b2 = "Food/Water per Stamina", "none"
        elif armor_name.lower() == "biker":
            b1, b2 = "Armor Rating", "Bike Fuel Use"
        elif armor_name.lower() == "enforcer":
            b1, b2 = ".44 Damage", ".44 Reload Speed"
        elif armor_name.lower() == "commando":
            b1, b2 = "Crit Heal Speed", "none"
        elif armor_name.lower() == "farmer":
            b1, b2 = "Food Health/Stamina Bonus", "none"
        elif armor_name.lower() == "lumberjack":
            b1, b2 = "Axe Stamina Cost", "Wood Harvest"
        elif armor_name.lower() == "miner":
            b1, b2 = "Mining Tool Durability Loss", "none"
        elif armor_name.lower() == "nerd":
            b1, b2 = "Tools/Weapon Durability Loss", "none"
        elif armor_name.lower() == "nomad":
            b1, b2 = "Stamina Regen Food/Water Cost", "none"
        elif armor_name.lower() == "preacher":
            b1, b2 = "Crit Resist", "Infection Resist"
        elif armor_name.lower() == "raider":
            b1, b2 = "Armor Crit Resist", "none"
        elif armor_name.lower() == "ranger":
            b1, b2 = "Revolver/Lever Reload Speed", "none"
        elif armor_name.lower() == "rogue":
            b1, b2 = "Dukes Loot Bonus", "none"
        elif armor_name.lower() == "scavenger":
            b1, b2 = "Loot Stage Bonus", "none"

        out[f"agfArmor{armor_name}SetBonus1"] = b1 or "none"
        out[f"agfArmor{armor_name}SetBonus2"] = b2 or "none"
    return out


def _active_armor_names_from_setbonus_map(armor_setbonus_loc: dict[str, str]) -> set[str]:
    """Derive active armor names from templated armorsTab set-bonus keys."""
    active: set[str] = set()
    for key in (armor_setbonus_loc or {}).keys():
        m = re.match(r"^agfArmor([A-Za-z0-9_]+)SetBonus(?:1|2|Desc)$", key or "")
        if m:
            active.add(m.group(1))
    return active


def _filter_armor_rows_to_active_sets(rows: dict[str, str], active_armor_names: set[str]) -> dict[str, str]:
    """Keep only armor localization rows that belong to active armorsTab sets."""
    if not rows or not active_armor_names:
        return rows

    out: dict[str, str] = {}
    for key, line in rows.items():
        m = re.match(
            r"^agfArmor([A-Za-z0-9_]+)(SetBonusDesc|SetBonus1|SetBonus2|HelmetDescription|OutfitDescription|GloveDescription|BootsDescription)$",
            key or "",
        )
        if not m:
            continue
        if m.group(1) in active_armor_names:
            out[key] = line
    return out


def _is_nonactive_armor_localization_key(key: str, active_armor_names: set[str] | None) -> bool:
    """True when key is armor-localization content for a set not present in armorsTab."""
    if not active_armor_names:
        return False
    m = re.match(
        r"^agf(?:5)?Armor([A-Za-z0-9_]+)(SetBonusDesc|SetBonus1|SetBonus2|HelmetDescription|Outfit|OutfitDescription|GloveDescription|BootsDescription)$",
        key or "",
    )
    if not m:
        return False
    return m.group(1) not in active_armor_names


def _is_setbonus_localization_key(key: str) -> bool:
    return bool(re.match(r"^agf(?:5)?Armor[A-Za-z0-9_]+SetBonus(?:Desc|[12])$", key or ""))


def parse_rocket_ammo_recipes(recipes_path: Path) -> set[str]:
    """Return rocket ammo recipe names so they always appear in Ammo Unlocks.

    These are rendered as locked/unlocked via recipe cvar fill state.
    """
    out: set[str] = set()
    try:
        tree = ET.parse(recipes_path)
    except (FileNotFoundError, ET.ParseError):
        return out
    root = tree.getroot()
    for rec in root.iter("recipe"):
        nm = rec.get("name", "")
        if not nm:
            continue
        nm_l = nm.lower()
        if nm_l.startswith("ammorocket") and not nm_l.startswith("ammobundlerocket"):
            out.add(nm)
    return out


def parse_default_ammo_recipes(recipes_path: Path) -> set[str]:
    """Return ammo recipe names that are not locked at game start.

    In vanilla recipes.xml, locked recipes carry the "learnable" tag and
    require a cvar/RecipeTagUnlocked path. Default-available recipes omit it.
    """
    out: set[str] = set()
    try:
        tree = ET.parse(recipes_path)
    except (FileNotFoundError, ET.ParseError):
        return out
    root = tree.getroot()
    for rec in root.iter("recipe"):
        nm = rec.get("name", "")
        if not nm:
            continue
        if _classify_unlock_category(nm) != "Ammo":
            continue
        tags = {t.strip().lower() for t in _split_csv(rec.get("tags", ""))}
        if "learnable" in tags:
            continue
        out.add(nm)
    return out


def parse_recipe_names(recipes_path: Path) -> tuple[set[str], bool]:
    """Return all recipe names (lowercased) and whether parsing succeeded."""
    out: set[str] = set()
    try:
        tree = ET.parse(recipes_path)
    except (FileNotFoundError, ET.ParseError):
        return out, False
    root = tree.getroot()
    for rec in root.iter("recipe"):
        nm = (rec.get("name") or "").strip().lower()
        if nm:
            out.add(nm)
    return out, True


def parse_quality_tier_count(qualityinfo_path: Path) -> int | None:
    """Return playable quality tier count from qualityinfo.xml (Q1..Qn)."""
    try:
        tree = ET.parse(qualityinfo_path)
    except (FileNotFoundError, ET.ParseError):
        return None

    root = tree.getroot()
    positive_keys: list[int] = []
    for q in root.iter("quality"):
        raw_key = (q.get("key") or "").strip()
        if not raw_key:
            continue
        try:
            key = int(raw_key)
        except ValueError:
            continue
        if key > 0:
            positive_keys.append(key)

    if not positive_keys:
        return None

    # Vanilla qualityinfo is keyed by quality level numbers.
    return max(positive_keys)


def _is_uncraftable_item_modifier(name: str, recipe_names: set[str], recipes_loaded: bool) -> bool:
    """True when an unlock icon is an item modifier without a craft recipe."""
    nm = (name or "").strip().lower()
    if not recipes_loaded:
        return False
    if not nm.startswith("mod"):
        return False
    return nm not in recipe_names


# Global cache populated in main()
ITEM_ICONS: dict[str, ItemIcon] = {}
ARMOR_VALUE_SERIES_BY_ITEM: dict[str, list[list[float]]] = {}
ARMOR_SETBONUS_NUMERIC_SERIES_BY_OUTFIT: dict[str, dict[int, list[float]]] = {}


def resolve_item_icon(item: str) -> ItemIcon:
    return ITEM_ICONS.get(item, ItemIcon(sprite=item))


def parse_progression(path: Path) -> list[Skill]:
    tree = ET.parse(path)
    root = tree.getroot()
    skills: list[Skill] = []
    for cs in root.iter("crafting_skill"):
        name = cs.get("name", "")
        if not name.startswith("crafting"):
            continue
        sk = Skill(
            name=name,
            max_level=int(cs.get("max_level", "100")),
            icon=cs.get("icon", "ui_game_symbol_book"),
        )
        for de in cs.findall("display_entry"):
            has_q = de.get("has_quality", "true").lower() != "false"
            unlock = [int(x.strip()) for x in de.get("unlock_level", "").split(",") if x.strip()]
            # Per-level item lists. unlock_entry's unlock_tier="N" is 1-indexed
            # into the parent unlock_level list. A single unlock_entry with
            # multiple items applies them all to that level.
            level_items: list[list[str]] = [[] for _ in unlock]
            for u in de.findall("unlock_entry"):
                tier_attr = u.get("unlock_tier") or u.get("level")
                items_attr = u.get("item", "")
                items = [s.strip() for s in items_attr.split(",") if s.strip()]
                if tier_attr and tier_attr.strip().isdigit():
                    tidx = int(tier_attr) - 1
                    if 0 <= tidx < len(level_items):
                        level_items[tidx].extend(items)
                        continue
                # No usable tier: spread across all levels equally (tiered rows)
                for lst in level_items:
                    lst.extend(items)
            # Flat fallback list (used by zoomed view & tiered single-icon column)
            child_items: list[str] = []
            for lst in level_items:
                for it in lst:
                    if it not in child_items:
                        child_items.append(it)
            if child_items:
                icons = child_items
            elif de.get("item"):
                icons = [s.strip() for s in de.get("item", "").split(",") if s.strip()]
            elif de.get("icon"):
                icons = [s.strip() for s in de.get("icon", "").split(",") if s.strip()]
            else:
                icons = []
            # If no unlock_entry children populated level_items, fall back to
            # spreading the icon/item attribute list across each level slot
            # (one item per level, in order).
            if not any(level_items) and icons:
                if len(icons) == len(unlock):
                    level_items = [[ic] for ic in icons]
                else:
                    # Tiered single item: same set at every level
                    level_items = [list(icons) for _ in unlock]
            names_attr = de.get("name_key") or ""
            names = [s.strip() for s in names_attr.split(",") if s.strip()]
            if not names:
                names = list(icons)
            # Per-level name keys: align positionally with level_items where
            # possible, else reuse the global names list.
            level_names: list[list[str]] = []
            if len(names) == len(unlock):
                # icon-list-style: one name per level
                level_names = [[names[i]] * max(1, len(level_items[i])) for i in range(len(unlock))]
            else:
                level_names = [list(names) for _ in unlock]
            sk.display.append(DisplayRow(
                items=icons,
                unlock_levels=unlock,
                name_keys=names,
                has_quality=has_q,
                level_items=level_items,
                level_names=level_names,
            ))
        # Note: do NOT merge armor T2/T3/T4 rows even though they share
        # unlock_levels Ã¢â‚¬â€ the user wants them as 4 distinct categories
        # (Primitive/Light/Medium/Heavy), each with its own row icon.
        # Special case: replace craftingArmor's display with exactly TWO
        # containers: Primitive (1 outfit icon) and a merged Light/Med/Heavy
        # container holding three representative outfit icons.
        if sk.name == "craftingArmor":
            light = ["armorAthleticOutfit", "armorEnforcerOutfit", "armorLumberjackOutfit",
                     "armorPreacherOutfit", "armorRogueOutfit"]
            medium = ["armorAssassinOutfit", "armorBikerOutfit", "armorCommandoOutfit",
                      "armorFarmerOutfit", "armorRangerOutfit", "armorScavengerOutfit"]
            heavy = ["armorMinerOutfit", "armorNerdOutfit", "armorNomadOutfit",
                     "armorRaiderOutfit"]
            sk.display = [
                DisplayRow(
                    items=["armorPrimitiveOutfit"],
                    unlock_levels=[1, 2, 4, 6, 8, 10],
                    name_keys=["armorT1"],
                    has_quality=True,
                    level_items=[["armorPrimitiveOutfit"]] * 6,
                    level_names=[["armorT1"]] * 6,
                ),
                DisplayRow(
                    items=["armorAthleticOutfit", "armorAssassinOutfit", "armorMinerOutfit"],
                    unlock_levels=[11, 20, 40, 60, 80, 100],
                    name_keys=["agf5ArmorsRatingLight", "agf5ArmorsRatingMedium", "agf5ArmorsRatingHeavy"],
                    has_quality=True,
                    level_items=[["armorAthleticOutfit", "armorAssassinOutfit", "armorMinerOutfit"]] * 6,
                    level_names=[["agf5ArmorsRatingLight", "agf5ArmorsRatingMedium", "agf5ArmorsRatingHeavy"]] * 6,
                    icon_rows=[light, medium, heavy],
                    icon_row_names=[light, medium, heavy],
                ),
            ]
        skills.append(sk)
    return skills


def _validate_progression_quality_rows(skills: list[Skill], expected_quality_tiers: int) -> list[str]:
    """Validate that has_quality rows align with quality tier count."""
    issues: list[str] = []
    for skill in skills:
        for row_index, row in enumerate(skill.display, start=1):
            if not row.has_quality:
                continue
            unlock_count = len(row.unlock_levels)
            if unlock_count != expected_quality_tiers:
                issues.append(
                    f"{skill.name}: display_entry[{row_index}] has_quality unlock count {unlock_count} "
                    f"!= quality tiers {expected_quality_tiers}"
                )
    return issues


def _migrate_category_key(key: str) -> str:
    """Normalize legacy localization keys to current agf-prefixed naming."""
    if not key:
        return key

    explicit = {
        "agfArmorLight": "agf5ArmorsRatingLight",
        "agfArmorMedium": "agf5ArmorsRatingMedium",
        "agfArmorHeavy": "agf5ArmorsRatingHeavy",
        "agfMainArmorsCategoryLight": "agf5ArmorsRatingLight",
        "agfMainArmorsCategoryMedium": "agf5ArmorsRatingMedium",
        "agfMainArmorsCategoryHeavy": "agf5ArmorsRatingHeavy",
        "checklistSchematicsTitle": "agf0PurpleBookButton",
        "agfMainOverviewHeaderHint": "agf1MainHeaderHint",
        "agfMainOverviewTabArmors": "agf1MainTabArmors",
        "agfMainOverviewTabBooks": "agf1MainTabBooks",
        "agfMainOverviewTabMagazines": "agf1MainTabMagazines",
        "agfMainOverviewTabOverview": "agf1MainTabOverview",
        "agfMainOverviewTabOverviewTooltip": "agf1MainTabOverviewTooltip",
        "agfMainOverviewTabUnlocks": "agf1MainTabUnlocks",
        "agfMainOverviewButtonTooltip": "agf0PurpleBookButtonTooltip",
        "statPhysicalDamageResist": "agf5ArmorsRatingStatPhysicalDamageResist",
        "statPhysicalDamageResistHeavy": "agf5ArmorsRatingStatPhysicalDamageResistHeavy",
        "statPhysicalDamageResistLight": "agf5ArmorsRatingStatPhysicalDamageResistLight",
        "statPhysicalDamageResistMedium": "agf5ArmorsRatingStatPhysicalDamageResistMedium",
        "agfStatPhysicalDamageResist": "agf5ArmorsRatingStatPhysicalDamageResist",
        "agfStatPhysicalDamageResistHeavy": "agf5ArmorsRatingStatPhysicalDamageResistHeavy",
        "agfStatPhysicalDamageResistLight": "agf5ArmorsRatingStatPhysicalDamageResistLight",
        "agfStatPhysicalDamageResistMedium": "agf5ArmorsRatingStatPhysicalDamageResistMedium",
    }
    mapped = explicit.get(key)
    if mapped:
        return mapped

    if key.startswith("AGF") and len(key) > 3:
        return f"agf{key[3].upper()}{key[4:]}"

    m_suffix = re.match(r"^([A-Za-z0-9_]+)AGF$", key)
    if m_suffix:
        core = m_suffix.group(1)
        if core:
            return f"agf{core[0].upper()}{core[1:]}"

    m_setbonus = re.match(r"^armor([A-Za-z0-9_]+)SetBonus(Desc|1|2)$", key)
    if m_setbonus:
        return f"agf5Armor{m_setbonus.group(1)}SetBonus{m_setbonus.group(2)}"

    m_legacy_armor_piece_desc = re.match(
        r"^agf(?!Armor)([A-Z][A-Za-z0-9_]+)(Helmet|Outfit|Glove|Boots)Description$",
        key,
    )
    if m_legacy_armor_piece_desc:
        return f"agf5Armor{m_legacy_armor_piece_desc.group(1)}{m_legacy_armor_piece_desc.group(2)}Description"

    m_legacy_armor_setbonus = re.match(r"^agf(?!Armor)([A-Z][A-Za-z0-9_]+)SetBonus$", key)
    if m_legacy_armor_setbonus:
        return f"agf5Armor{m_legacy_armor_setbonus.group(1)}SetBonus"

    m_agf_armor_content = re.match(r"^agfArmor([A-Za-z0-9_]+)$", key)
    if m_agf_armor_content:
        return f"agf5Armor{m_agf_armor_content.group(1)}"

    if key.startswith("agfMainUnlocksCategory"):
        return "agf4UnlocksCategory" + key[len("agfMainUnlocksCategory"):]

    if key.startswith("agfArmorsRating"):
        return "agf5ArmorsRating" + key[len("agfArmorsRating"):]

    if key == "agfArmorSetBonusTooltip":
        return "agf5ArmorSetBonusTooltip"

    if key.startswith("agfUnlock") and key != "agfUnlocks":
        return "agf4UnlocksCategory" + key[len("agfUnlock"):]
    return key

# ---------------------------------------------------------------------------
# XML emission helpers
# ---------------------------------------------------------------------------

class W:
    """Tiny indent-aware XML writer for hand-shaped xui markup."""

    def __init__(self) -> None:
        self.lines: list[str] = []
        self.depth = 0

    def open(self, tag: str, **attrs) -> None:
        self.lines.append(self._indent() + self._tag(tag, attrs, self_close=False))
        self.depth += 1

    def close(self, tag: str) -> None:
        self.depth -= 1
        self.lines.append(self._indent() + f"</{tag}>")

    def leaf(self, tag: str, **attrs) -> None:
        self.lines.append(self._indent() + self._tag(tag, attrs, self_close=True))

    def comment(self, text: str) -> None:
        self.lines.append(self._indent() + f"<!--{text}-->")

    def raw(self, text: str) -> None:
        self.lines.append(self._indent() + text)

    def _indent(self) -> str:
        return "    " * self.depth

    def _tag(self, tag: str, attrs: dict, self_close: bool) -> str:
        parts = [tag]
        for k, v in attrs.items():
            k = k.rstrip("_").replace("__", ":")  # allow class_ etc
            parts.append(f'{k}="{v}"')
        body = " ".join(parts)
        return f"<{body}/>" if self_close else f"<{body}>"

    def render(self) -> str:
        return "\n".join(self.lines) + "\n"

# ---------------------------------------------------------------------------
# Layout primitives
# ---------------------------------------------------------------------------

def emit_qsection_tier6(w: W, skill: str, levels: list[int], items: list[str],
                        names: list[str] | None = None,
                        icon_rows: list[list[str]] | None = None,
                        icon_row_names: list[list[str]] | None = None) -> None:
    """One QSection (300Ãƒâ€”100 by default) with 6 quality chips, 6-step gradient,
    and icons centered.

    If ``icon_rows`` is provided, the section becomes taller (50px per extra
    icon row) and icons are emitted left-aligned at 50-stride per row.
    """
    if names is None:
        names = list(items)
    has_multi = bool(icon_rows)
    n_icon_rows = len(icon_rows) if has_multi else 1
    sec_h = 100 if not has_multi else 50 + 50 * n_icon_rows
    w.open("entry", name="QSection")
    w.comment("Background of Section")
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=300, height=sec_h, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=300, height=sec_h, color="[black]",
           type="sliced", fillcenter="false")
    w.comment("If completed (cumulative gradient)")
    for i, lvl in enumerate(levels):
        width = 50 * (i + 1)
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               globalopacity="false", pos="0,0", width=width, height=sec_h,
               fillcenter="true", type="filled",
               fill=f"{{cvar({skill}Check{lvl})}}")
    for i, lvl in enumerate(levels):
        x = 50 * i
        w.comment(f"Q{i+1}")
        w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"{x},0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"{x},0", width=50, height=30, color=QCOLORS[i], type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"{x+25},-15", pivot="center",
               width=50, height=30,
               justify="center", text=str(lvl), color="[black]", font_size=26)
    w.comment("Icon(s)")
    if has_multi:
        # Multi-row icon layout: each row left-aligned at 50-stride, packed.
        for r, row_items in enumerate(icon_rows):
            row_names = icon_row_names[r] if icon_row_names and r < len(icon_row_names) else list(row_items)
            iy = -(40 + r * 50)
            for i, it in enumerate(row_items):
                ix = i * 50  # leave 1px slop in hand-coded; close enough at 50
                tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (row_names[i] if i < len(row_names) and row_names[i] else it))
                ic = resolve_item_icon(it)
                w.leaf("sprite", name="itemIcon", depth=3, pos=f"{ix},{iy}", size="50,50",
                       foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                       color=ic.tint, tooltip_key=tk)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{ix},{iy}", width=20, height=20,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
    else:
        m = len(items) if items else 0
        if m > 0:
            # Pack icons as a tight group, centered horizontally in the 300-wide cell.
            icon_size = 50
            gap = 10
            group_w = m * icon_size + (m - 1) * gap
            start_x = (300 - group_w) // 2
            for i, it in enumerate(items):
                cx = start_x + i * (icon_size + gap)
                tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
                ic = resolve_item_icon(it)
                w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                       foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                       color=ic.tint, tooltip_key=tk)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{cx},-40", width=20, height=20,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("entry")


def emit_qsection_nup(w: W, skill: str, levels: list[int], items: list[str], names: list[str],
                      cell_width: int = 300,
                      level_items: list[list[str]] | None = None,
                      level_names: list[list[str]] | None = None) -> None:
    """N-up split (non-tiered) section.  Sub-cells share equal width with 2px gutter.

    If ``level_items``/``level_names`` are provided (list-per-slot), each
    sub-cell shows its own items centered horizontally inside the sub-cell;
    otherwise the flat ``items`` list is centered across the whole cell.
    """
    n = len(levels)
    if n <= 0:
        return
    gutter = 2
    sub_w = (cell_width - gutter * (n - 1)) // n
    used_w = sub_w * n + gutter * (n - 1)
    start_x = max(0, (cell_width - used_w) // 2)
    w.open("entry", name="QSection")
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=cell_width, height=100, color="[darkGrey]", type="sliced", fillcenter="true")
    for i, lvl in enumerate(levels):
        x = start_x + i * (sub_w + gutter)
        w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
               foregroundlayer="true", pos=f"{x},0", width=sub_w, height=100,
               color="[black]", type="sliced", fillcenter="false")
    for i, lvl in enumerate(levels):
        x = start_x + i * (sub_w + gutter)
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               pos=f"{x},0", width=sub_w, height=100, fillcenter="true",
               type="filled", fill=f"{{cvar({skill}Check{lvl})}}")
    for i, lvl in enumerate(levels):
        x = start_x + i * (sub_w + gutter)
        # Chip is anchored to the top-left of the sub-cell.
        chip_x = x
        chip_color = QCOLORS[0]
        w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"{chip_x},0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"{chip_x},0", width=50, height=30, color=chip_color, type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"{chip_x+25},-15", pivot="center",
               width=50, height=30,
               justify="center", text=str(lvl), color="[black]", font_size=26)
    # Icon placement
    icon_size = 50
    gap = 10
    if level_items:
        # Per-sub-cell: center each slot's icons inside its own sub-cell.
        for i, lvl in enumerate(levels):
            slot_items = level_items[i] if i < len(level_items) else []
            slot_names = level_names[i] if level_names and i < len(level_names) else list(slot_items)
            m = len(slot_items)
            if m == 0:
                continue
            x = start_x + i * (sub_w + gutter)
            # Shrink the inter-icon gap if the group would overflow the sub-cell.
            slot_gap = gap
            if m > 1:
                max_gap = (sub_w - 4 - m * icon_size) // (m - 1)
                if max_gap < gap:
                    slot_gap = max(0, max_gap)
            group_w = m * icon_size + (m - 1) * slot_gap
            icon_start_x = x + (sub_w - group_w) // 2
            for j, it in enumerate(slot_items):
                cx = icon_start_x + j * (icon_size + slot_gap)
                tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (slot_names[j] if j < len(slot_names) and slot_names[j] else it))
                ic = resolve_item_icon(it)
                w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                       foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                       color=ic.tint, tooltip_key=tk)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{cx},-40", width=20, height=20,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
    elif items:
        m = len(items)
        group_w = m * icon_size + (m - 1) * gap
        start_x = (cell_width - group_w) // 2
        for i, it in enumerate(items):
            cx = start_x + i * (icon_size + gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                   foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                   color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},-40", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("entry")


def emit_qsection_single(w: W, skill: str, level: int, items: list[str],
                         names: list[str] | None = None,
                         cell_width: int = 300,
                         section_height: int = 100) -> None:
    """A single-slot non-tiered section: 1 chip (top-left) + icons (centered).

    Used by the auto-layout zoomed-view grid where each unlock threshold gets
    its own ``cell_width \u00d7 100`` container.
    """
    if names is None:
        names = list(items)
    w.open("entry", name="QSection")
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
            width=cell_width, height=section_height, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
            foregroundlayer="true", pos="0,0", width=cell_width, height=section_height,
           color="[black]", type="sliced", fillcenter="false")
    w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
            pos="0,0", width=cell_width, height=section_height, fillcenter="true",
           type="filled", fill=f"{{cvar({skill}Check{level})}}")
    # Chip top-left, Q1 brown
    w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
           pos="0,0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
    w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
           pos="0,0", width=50, height=30, color=QCOLORS[0], type="sliced", fillcenter="true")
    w.leaf("label", depth=3, name="checkunlock", pos="25,-15", pivot="center",
           width=50, height=30, justify="center", text=str(level),
           color="[black]", font_size=26)
    # Icons centered
    m = len(items)
    if m > 0:
        max_per_row = 6
        h_gap = 6
        v_gap = 8 if section_height > 100 else 4

        if m <= max_per_row:
            icon_size = 50
            slot_gap = 10
            if m > 1:
                max_gap = (cell_width - 4 - m * icon_size) // (m - 1)
                if max_gap < slot_gap:
                    slot_gap = max(0, max_gap)
            group_w = m * icon_size + (m - 1) * slot_gap
            start_x = (cell_width - group_w) // 2
            # Center within the space below the top-left level chip (30px
            # tall) rather than the whole cell, so a single row of icons
            # doesn't crowd/overlap the chip and sits a bit lower instead.
            chip_h = 30
            extra = max(0, (section_height - chip_h) - icon_size)
            top_pad = chip_h + extra // 2
            y_positions = [-top_pad] * m
            x_positions = [start_x + j * (icon_size + slot_gap) for j in range(m)]
        else:
            cols = min(max_per_row, m)
            rows = math.ceil(m / cols)
            # Prefer keeping icon size at 50 for wrapped rows by reducing vertical
            # gap first, then shrinking icon size only if still required.
            if rows > 1 and section_height <= 100:
                max_v_gap_for_50 = (section_height - rows * 50) // (rows - 1)
                if max_v_gap_for_50 >= 0:
                    v_gap = min(v_gap, max_v_gap_for_50)

            max_icon_w = (cell_width - 4 - h_gap * (cols - 1)) // cols
            max_icon_h = (section_height - v_gap * (rows - 1)) // rows
            icon_size = max(18, min(50, max_icon_w, max_icon_h))
            total_h = rows * icon_size + (rows - 1) * v_gap
            top_pad = max(0, (section_height - total_h) // 2)

            x_positions: list[int] = []
            y_positions: list[int] = []
            idx = 0
            for r in range(rows):
                row_count = min(cols, m - idx)
                row_w = row_count * icon_size + (row_count - 1) * h_gap
                row_x = max(0, (cell_width - row_w) // 2)
                y = -(top_pad + r * (icon_size + v_gap))
                for c in range(row_count):
                    x_positions.append(row_x + c * (icon_size + h_gap))
                    y_positions.append(y)
                idx += row_count

        for j, it in enumerate(items):
            cx = x_positions[j]
            cy = y_positions[j]
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[j] if j < len(names) and names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{cy}", size=f"{icon_size},{icon_size}",
                   foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                   color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                type_size = 20 if icon_size >= 40 else max(8, int(icon_size * 0.4))
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{cy}", width=type_size, height=type_size,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("entry")


def _emit_single_section_rect(w: W, skill: str, level: int,
                              items: list[str], names: list[str],
                              x: int, y: int,
                              width: int = 300, height: int = 100,
                              row1_count: int | None = None,
                              row1_col_offset: int = 0,
                              force_cols: int | None = None,
                              row2_extra_drop: int = 0) -> None:
    """Render one non-tiered section as a positioned rect.

    When ``row1_count`` is provided, icons are packed into two rows inside the
    icon area to the right of the 50px chip.
    """
    w.open("rect", name=f"QSection{level}", pos=f"{x},{y}", width=width, height=height)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=width, height=height, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=width, height=height,
           color="[black]", type="sliced", fillcenter="false")
    w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
           pos="0,0", width=width, height=height, fillcenter="true",
           type="filled", fill=f"{{cvar({skill}Check{level})}}")
    w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
           pos="0,0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
    w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
           pos="0,0", width=50, height=30, color=QCOLORS[0], type="sliced", fillcenter="true")
    w.leaf("label", depth=3, name="checkunlock", pos="25,-15", pivot="center",
           width=50, height=30, justify="center", text=str(level),
           color="[black]", font_size=26)

    if not items:
        w.close("rect")
        return

    if row1_count is None:
        # Default single-row center for rect-based sections.
        icon_size = 50
        gap = 10
        m = len(items)
        group_w = m * icon_size + (m - 1) * gap
        start_x = (width - group_w) // 2
        y_pos = -40 if height <= 100 else -52
        for j, it in enumerate(items):
            cx = start_x + j * (icon_size + gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[j] if j < len(names) and names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{y_pos}",
                   size="50,50", foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{y_pos}", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    else:
        split = max(0, min(len(items), row1_count))
        top_items = items[:split]
        bot_items = items[split:]
        top_names = names[:split]
        bot_names = names[split:]
        icon_size = 50
        type_sz = 20
        preferred_gap = 8
        row_gap = 8
        ncols = max(len(bot_items), len(top_items) + max(0, row1_col_offset))
        if force_cols is not None:
            ncols = max(ncols, force_cols)
        if ncols <= 0:
            w.close("rect")
            return
        area_left = 55
        area_right_pad = 5
        area_w = max(10, width - area_left - area_right_pad)
        if ncols <= 1:
            col_gap = 0
        else:
            col_gap = max(0, min(preferred_gap, (area_w - ncols * icon_size) // (ncols - 1)))
        grid_w = ncols * icon_size + (ncols - 1) * col_gap
        start_x = area_left + (area_w - grid_w) // 2
        two_row_h = icon_size * 2 + row_gap
        top_pad = max(0, (height - two_row_h) // 2)
        y_top = -top_pad
        y_bot = y_top - icon_size - row_gap - row2_extra_drop

        for j, it in enumerate(top_items):
            col = j + max(0, row1_col_offset)
            cx = start_x + col * (icon_size + col_gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (top_names[j] if j < len(top_names) and top_names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{y_top}",
                   size="50,50", foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{y_top}", width=type_sz, height=type_sz,
                       sprite=ic.type_icon, foregroundlayer="true", color="")

        for j, it in enumerate(bot_items):
            cx = start_x + j * (icon_size + col_gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (bot_names[j] if j < len(bot_names) and bot_names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{y_bot}",
                   size="50,50", foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{y_bot}", width=type_sz, height=type_sz,
                       sprite=ic.type_icon, foregroundlayer="true", color="")

    w.close("rect")


def _emit_tier6_section_rect(w: W, skill: str, levels: list[int],
                             items: list[str], names: list[str],
                             x: int, y: int,
                             width: int = 300, height: int = 100) -> None:
    """Render a 6-quality tiered section as a positioned rect."""
    w.open("rect", name="QSectionTier6", pos=f"{x},{y}", width=width, height=height)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=width, height=height, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=width, height=height,
           color="[black]", type="sliced", fillcenter="false")
    for i, lvl in enumerate(levels[:6]):
        fill_w = 50 * (i + 1)
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               globalopacity="false", pos="0,0", width=fill_w, height=height,
               fillcenter="true", type="filled", fill=f"{{cvar({skill}Check{lvl})}}")
    for i, lvl in enumerate(levels[:6]):
        cx = 50 * i
        w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"{cx},0", width=50, height=30, color="[black]", type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"{cx},0", width=50, height=30, color=QCOLORS[i], type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"{cx+25},-15", pivot="center",
               width=50, height=30, justify="center", text=str(lvl), color="[black]", font_size=26)
    if items:
        m = len(items)
        icon_size = 50
        gap = 10
        group_w = m * icon_size + (m - 1) * gap
        start_x = (width - group_w) // 2
        for j, it in enumerate(items):
            cx = start_x + j * (icon_size + gap)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[j] if j < len(names) and names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},-40", size="50,50",
                   foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                   color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},-40", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("rect")


def _emit_tier6_section_vertical_rect(w: W, skill: str, levels: list[int],
                                      items: list[str], names: list[str],
                                      x: int, y: int,
                                      width: int = 180, height: int = 375) -> None:
    """Render a vertical/transposed 6-quality section (ALL-page style, larger)."""
    w.open("rect", name="QSectionTier6Vertical", pos=f"{x},{y}", width=width, height=height)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=width, height=height, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", pos="0,0", width=width, height=height,
           color="[black]", type="sliced", fillcenter="false")

    seg_h = max(1, height // 6)
    for i, lvl in enumerate(levels[:6]):
        fill_h = min(height, seg_h * (i + 1))
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               globalopacity="false", pos="0,0", width=width, height=fill_h,
               fillcenter="true", type="filled", fill=f"{{cvar({skill}Check{lvl})}}")

    chip_h = min(30, max(20, seg_h - 2))
    # Place chips so the final (100) chip bottom edge meets the section bottom.
    span = max(1, height - chip_h)
    for i, lvl in enumerate(levels[:6]):
        cy = -int(round(i * span / 5.0))
        w.leaf("sprite", depth=3, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"0,{cy}", width=50, height=chip_h, color="[black]", type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"0,{cy}", width=50, height=chip_h, color=QCOLORS[i], type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"25,{cy - chip_h // 2}", pivot="center",
               width=50, height=chip_h, justify="center", text=str(lvl), color="[black]", font_size=26)

    if items:
        m = len(items)
        icon_size = 50
        gap = 10
        right_x = 50
        right_w = max(icon_size, width - right_x)
        content_h = m * icon_size + (m - 1) * gap
        start_y = -((height - content_h) // 2)
        for j, it in enumerate(items):
            cy = start_y - j * (icon_size + gap)
            cx = right_x + max(0, (right_w - icon_size) // 2)
            tk = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[j] if j < len(names) and names[j] else it))
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3, pos=f"{cx},{cy}", size="50,50",
                   foregroundlayer="true", atlas="ItemIconAtlas", sprite=ic.sprite,
                   color=ic.tint, tooltip_key=tk)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{cx},{cy}", width=20, height=20,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("rect")

# ---------------------------------------------------------------------------
# Magazine top-strip (shared across all 23 zoomed views)
# ---------------------------------------------------------------------------

def emit_magazine_strip(w: W, skills: list[Skill]) -> None:
    """The 23-icon row across the top of every zoomed<Cat> rect."""
    w.open("grid", name="tabButtons", pos="5,-10", depth=3, rows=1, cols=23,
           cell_width=60, cell_height=100, repeat_content="false",
           arrangement="horizontal", controller="MapStats")
    by_name = {s.name: s for s in skills}
    for tab_skill in TAB_ORDER:
        sk = by_name.get(tab_skill)
        if not sk:
            continue
        w.open("entry")
        w.open("rect", depth=10, pos="5,0", width=50, height=100)
        w.leaf("sprite", depth=10, name="itemIcon", pos="2,-9", width=50, height=50,
               size="46,46", foregroundlayer="true", atlas="ItemIconAtlas",
               sprite=sk.book_sprite, color="")
        w.leaf("label", depth=10, name="check", pos="0,-60", width=50, height=16,
               justify="center", text=f"[fc8c03]{{cvar({sk.name}Check)}}[-]", font_size=22)
        w.leaf("label", depth=10, name="check", pos="0,-77", width=50, height=16,
               justify="center", text=f"/{sk.max_level}", font_size=22)
        w.leaf("filledsprite", depth=1, name="yesUnlocked", color="10,255,10,65",
               pos="2,-1", width=46, height=98, fillcenter="true", type="filled",
               fill=f"{{cvar({sk.name}Check{sk.max_level})}}")
        w.close("rect")
        w.close("entry")
    w.close("grid")

# ---------------------------------------------------------------------------
# Compact strip + zoomed view
# ---------------------------------------------------------------------------

def _collect_non_tiered_slots(sk: Skill) -> list[tuple[int, list[str], list[str]]]:
    """Flatten non-tiered display rows into ordered (level, items, names) slots."""
    slots: list[tuple[int, list[str], list[str]]] = []
    for row in sk.display:
        if row.is_tiered:
            continue
        for i, lvl in enumerate(row.unlock_levels):
            items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
            if not items:
                items = list(row.items)
            names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
            slots.append((lvl, items, names))
    return slots


def _group_slots_by_unlock_level(
    slots: list[tuple[int, list[str], list[str]]]
) -> list[tuple[int, list[str], list[str]]]:
    """Merge slot items by unlock level while preserving first-seen order."""
    grouped_items: dict[int, list[str]] = {}
    grouped_names: dict[int, list[str]] = {}

    for lvl, items, names in slots:
        if lvl not in grouped_items:
            grouped_items[lvl] = []
            grouped_names[lvl] = []
        out_items = grouped_items[lvl]
        out_names = grouped_names[lvl]
        for idx, item in enumerate(items):
            if not item or item in out_items:
                continue
            out_items.append(item)
            nm = names[idx] if idx < len(names) and names[idx] else item
            out_names.append(nm)

    return [(lvl, grouped_items[lvl], grouped_names[lvl]) for lvl in grouped_items.keys()]

def emit_compact_strip(w: W, sk: Skill, x: int) -> None:
    """The narrow 50px column shown in the All-Checklists view.
    The Magazine header (icon + level/max + greenfill) lives in the
    tabsHeader buttons themselves, so the strip starts directly with
    Section1 just under the buttons."""
    short_n = sk.short_name

    if sk.name == "craftingSeeds":
        # Non-quality behavior: section grouping follows unlock levels.
        seeds_slots = _group_slots_by_unlock_level(_collect_non_tiered_slots(sk))
        total_h = 0
        for _lvl, items, _names in seeds_slots:
            total_h += _nontiered_height(max(1, len(items)))

        w.open("rect", name=f"checklist{short_n}", depth=3, pos=f"{x},-110", width=50, height=total_h, controller="MapStats")
        y_cursor = 0
        section_idx = 0
        for lvl, items, names in seeds_slots:
            section_idx += 1
            synthetic = DisplayRow(
                items=list(items),
                unlock_levels=[lvl],
                name_keys=list(names),
                has_quality=False,
                level_items=[list(items)],
                level_names=[list(names)],
            )
            sec_h = _emit_nontiered_section(w, sk, synthetic, 0, lvl, section_idx, y_cursor)
            y_cursor -= sec_h
        w.close("rect")
        return

    # Pre-compute total column height as the sum of section heights.
    total_h = 0
    food_compact = (sk.name == "craftingFood")
    for row in sk.display:
        if row.has_quality and len(row.unlock_levels) == 6:
            n_icons = max(1, len(row.items))
            total_h += max(120, 10 + 25 * n_icons + 5)
        else:
            for i in range(len(row.unlock_levels)):
                items_here = row.level_items[i] if i < len(row.level_items) else row.items
                n = max(1, len(items_here))
                if food_compact:
                    sec_h, _ = _food_compact_layout(n)
                    total_h += sec_h
                else:
                    total_h += _nontiered_height(n)
    # y=-110 places the strip directly below the 100px tab buttons (which
    # sit at y=-10..-110). Sections start at y=0 within this rect.
    w.open("rect", name=f"checklist{short_n}", depth=3, pos=f"{x},-110", width=50, height=total_h, controller="MapStats")
    # Section1..N Ã¢â‚¬â€ one per display row, with cumulative y-offset so taller
    # sections (multi-icon rows) push later sections down.
    y_cursor = 0
    section_idx = 0
    for row in sk.display:
        if row.has_quality and len(row.unlock_levels) == 6:
            section_idx += 1
            sec_h = _emit_tiered_section(w, sk, row, section_idx, y_cursor)
            y_cursor -= sec_h
        else:
            for li, lvl in enumerate(row.unlock_levels):
                section_idx += 1
                sec_h = _emit_nontiered_section(w, sk, row, li, lvl, section_idx, y_cursor)
                y_cursor -= sec_h
    w.close("rect")


def _icon_slots(n: int) -> list[tuple[int, int]]:
    """Return (x,y) positions for `n` icons in a non-tiered Section.
    Special-cases:
      n=1 -> single icon centered with shrunk container, at (26,-5)
      n=2 -> vertical stack at (26,-5), (26,-25) (single column, tight)
      n>=3 -> alternating left/right grid pattern matching the hand-built book.
    """
    if n <= 0:
        return []
    if n == 1:
        return [(26, -5)]
    if n == 2:
        return [(26, -5), (26, -25)]
    slots: list[tuple[int, int]] = [(26, -10)]
    for r in range(1, 16):
        y = -10 - r * 25
        slots.append((2, y))
        slots.append((26, y))
        if len(slots) >= n:
            break
    return slots[:n]


def _nontiered_height(n: int) -> int:
    """Container height (px) for a non-tiered Section with n icons.
    Includes ~5px of bottom padding so icons don't kiss the bottom border
    (matches the tiered-container look).
    """
    if n <= 1:
        return 30
    if n == 2:
        return 51
    if n == 3:
        return 61
    if n <= 5:
        return 86
    if n <= 7:
        return 111
    if n <= 9:
        return 136
    rows = (n + 2) // 2
    return 10 + rows * 25 + 6


def _food_compact_layout(n: int) -> tuple[int, list[tuple[int, int, int]]]:
    """Compact Food-strip icon layout for one unlock-level chip.

    n=1 keeps the original full-size centered icon; n=2 keeps the original
    stacked pair. n>=3 (new items added by a game update landing in the same
    unlock level as an existing pair) packs into a 2-column grid at the same
    small icon size as the n=2 case, so extra items shrink to fit instead of
    growing the row far taller than its neighbors in the compact strip.

    Returns (section_height, [(x, y, size), ...]) with one tuple per item.
    """
    if n <= 0:
        return 30, []
    if n == 1:
        return 30, [(26, -5, 21)]
    if n == 2:
        return 30, [(31, -4, 11), (31, -15, 11)]
    cols = 2
    rows = math.ceil(n / cols)
    icon_size = 11
    row_gap = 3
    col_gap = 2
    area_left, area_w = 24, 26
    content_h = rows * icon_size + (rows - 1) * row_gap
    sec_h = max(30, content_h + 4)
    top_pad = max(2, (sec_h - content_h) // 2)
    positions: list[tuple[int, int, int]] = []
    idx = 0
    for r in range(rows):
        row_count = min(cols, n - idx)
        row_w = row_count * icon_size + max(0, row_count - 1) * col_gap
        row_x = area_left + max(0, (area_w - row_w) // 2)
        y = -(top_pad + r * (icon_size + row_gap))
        for c in range(row_count):
            positions.append((row_x + c * (icon_size + col_gap), y, icon_size))
            idx += 1
    return sec_h, positions


def _emit_icon_grid(w: W, items: list[str], names: list[str]) -> int:
    """Emit icons inside a Section rect. Returns the grid's content height.
    See `_icon_slots` for the placement pattern."""
    n = len(items)
    slots = _icon_slots(n)
    for i in range(n):
        it = items[i]
        x, y = slots[i]
        # Prefer the entity name as tooltip_key (items.xml/blocks.xml entries
        # are also loc keys in vanilla). Fall back to name_key only when the
        # entity isn't in our parsed dicts. TOOLTIP_OVERRIDES wins.
        tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
        ic = resolve_item_icon(it)
        w.leaf("sprite", name="itemIcon", depth=3,
               pos=f"{x},{y}", size="21,21",
               foregroundlayer="true", atlas="ItemIconAtlas",
               sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
        if ic.type_icon:
            w.leaf("sprite", name="itemtypeicon", depth=8,
                   pos=f"{x},{y}", width=10, height=10,
                   sprite=ic.type_icon, foregroundlayer="true", color="")
    # Content height: handled by caller via _nontiered_height
    return _nontiered_height(n)


def _emit_tiered_section(w: W, sk: Skill, row: 'DisplayRow', idx: int, y: int) -> int:
    """Emit a 6-quality container at y offset `y`. Returns its height."""
    items = list(row.items)
    names = list(row.name_keys)
    n_icons = max(1, len(items))
    sec_h = max(120, 10 + 25 * n_icons + 5)
    w.open("rect", name=f"Section{idx}", pos=f"0,{y}",
           width=50, height=sec_h)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=50, height=sec_h + 2, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", width=50, height=sec_h + 2, color="[black]",
           type="sliced", fillcenter="false")
    for i, lvl in enumerate(row.unlock_levels[:6]):
        h = 20 * (i + 1)
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               pos="0,0", width=50, height=h, fillcenter="true", type="filled",
               fill=f"{{cvar({sk.name}Check{lvl})}}")
    for i, lvl in enumerate(row.unlock_levels[:6]):
        by = -2 - i * 20
        iy = -3 - i * 20
        color = QCOLORS[i]
        w.leaf("sprite", depth=2, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"2,{by}", width=21, height=18, color="[black]",
               type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"3,{iy}", width=19, height=16, color=color,
               type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"0,{iy}",
               width=25, height=16, justify="center", text=str(lvl),
               color="[black]", font_size=16)
    # Tiered icon column: vertical stack starting at y=-10
    for i, it in enumerate(items):
        iy = -10 - i * 25
        tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
        ic = resolve_item_icon(it)
        w.leaf("sprite", name="itemIcon", depth=3,
               pos=f"26,{iy}", size="21,21",
               foregroundlayer="true", atlas="ItemIconAtlas",
               sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
        if ic.type_icon:
            w.leaf("sprite", name="itemtypeicon", depth=8,
                   pos=f"26,{iy}", width=10, height=10,
                   sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("rect")
    return sec_h


def _walk_nontiered(row: 'DisplayRow'):
    """Walk a non-tiered row's unlock_levels and yield either:
      ("merged", [i, i+1, ...])  - run of >=2 consecutive single-icon levels
      ("single", i)              - one level (multi-icon, or a lone single)
    """
    n = len(row.unlock_levels)
    i = 0
    while i < n:
        items_here = row.level_items[i] if i < len(row.level_items) else row.items
        if len(items_here) == 1:
            j = i
            while j < n:
                items_j = row.level_items[j] if j < len(row.level_items) else row.items
                if len(items_j) != 1:
                    break
                j += 1
            run = list(range(i, j))
            if len(run) >= 2:
                yield ("merged", run)
            else:
                yield ("single", i)
            i = j
        else:
            yield ("single", i)
            i += 1


def _merged_singles_height(n: int) -> int:
    """Height of a merged-singles container with n tiers (1 icon per tier)."""
    return 22 * n + 5


def _emit_merged_singles_section(w: W, sk: Skill, row: 'DisplayRow',
                                 indices: list[int], idx: int, y: int) -> int:
    """Emit ONE container holding N consecutive single-icon tiers stacked
    vertically. Each row = (Q1 brown chip, level number, single icon) and has
    its own green-fill driven by ``cvar({sk}Check{lvl})``. Saves height vs
    emitting N separate 30-px containers.
    """
    pitch = 22
    n = len(indices)
    sec_h = _merged_singles_height(n)
    w.open("rect", name=f"Section{idx}", pos=f"0,{y}", width=50, height=sec_h)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=50, height=sec_h + 2, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", width=50, height=sec_h + 2, color="[black]",
           type="sliced", fillcenter="false")
    # Pass 1: per-row green fills (drawn before chip backgrounds so chips win)
    for slot, li in enumerate(indices):
        lvl = row.unlock_levels[li]
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               pos=f"0,{-slot*pitch}", width=50, height=pitch, fillcenter="true",
               type="filled", fill=f"{{cvar({sk.name}Check{lvl})}}")
    # Pass 2: chips + icons
    for slot, li in enumerate(indices):
        lvl = row.unlock_levels[li]
        items_here = row.level_items[li] if li < len(row.level_items) else row.items
        names_here = row.level_names[li] if li < len(row.level_names) else row.name_keys
        it = items_here[0] if items_here else ""
        if it:
            tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names_here[0] if names_here and names_here[0] else it))
        else:
            tip = ""
        by = -2 - slot * pitch
        iy = -3 - slot * pitch
        w.leaf("sprite", depth=2, name="backgroundBorder", sprite="menu_empty3px",
               pos=f"2,{by}", width=21, height=18, color="[black]",
               type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos=f"3,{iy}", width=19, height=16, color=QCOLORS[0],
               type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos=f"0,{iy}",
               width=25, height=16, justify="center", text=str(lvl),
               color="[black]", font_size=16)
        if it:
            ic = resolve_item_icon(it)
            w.leaf("sprite", name="itemIcon", depth=3,
                   pos=f"26,{iy}", size="21,21",
                   foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"26,{iy}", width=10, height=10,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
    w.close("rect")
    return sec_h


def _emit_nontiered_section(w: W, sk: Skill, row: 'DisplayRow',
                            li: int, lvl: int, idx: int, y: int) -> int:
    """Emit a single-Q-chip container holding row's items unlocked at this
    specific level (row.level_items[li]). Container height is sized to fit
    the icons. Returns the height used."""
    items = list(row.level_items[li]) if li < len(row.level_items) else list(row.items)
    if not items:
        items = list(row.items)
    names = list(row.level_names[li]) if li < len(row.level_names) else list(row.name_keys)
    n = max(1, len(items))
    # Compact layout for Food: chip + icons packed into the narrow right cell.
    # Keeps one container per unlock level so organization is intact; extra
    # items (n>=3, e.g. a game update adding a new item to an existing level)
    # shrink to fit via _food_compact_layout instead of falling back to the
    # much taller generic icon grid.
    food_compact = (sk.name == "craftingFood")
    if food_compact:
        sec_h, icon_positions = _food_compact_layout(n)
        w.open("rect", name=f"Section{idx}", pos=f"0,{y}", width=50, height=sec_h)
        w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
               width=50, height=sec_h + 2, color="[darkGrey]", type="sliced", fillcenter="true")
        w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
               foregroundlayer="true", width=50, height=sec_h + 2, color="[black]",
               type="sliced", fillcenter="false")
        w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
               pos="0,0", width=50, height=sec_h, fillcenter="true", type="filled",
               fill=f"{{cvar({sk.name}Check{lvl})}}")
        # Chip pinned to top-left (consistent with all other sections)
        w.leaf("sprite", depth=2, name="backgroundBorder", sprite="menu_empty3px",
               pos="2,-2", width=21, height=18, color="[black]",
               type="sliced", fillcenter="false")
        w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
               pos="3,-3", width=19, height=16, color=QCOLORS[0],
               type="sliced", fillcenter="true")
        w.leaf("label", depth=3, name="checkunlock", pos="0,-3",
               width=25, height=16, justify="center", text=str(lvl),
               color="[black]", font_size=16)
        # Right cell = x[24..50] = 26 wide, full 30 px tall.
        # n=1: full-size 21x21 icon centered in right cell.
        # n=2: vertical stack of two 14x14 icons, centered horizontally and
        # vertically inside the right cell (no border overlap).
        if n == 1:
            it = items[0]
            tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[0] if names and names[0] else it))
            ic = resolve_item_icon(it)
            # Match _icon_slots(1) exactly: pos=(26,-5), size=21x21
            ix, iy = 26, -5
            ov = FOOD_ICON_OVERRIDES.get(ic.sprite) or FOOD_ICON_OVERRIDES.get(it) or {}
            ix += ov.get("dx", 0); iy += ov.get("dy", 0)
            sz = ov.get("size", 21)
            w.leaf("sprite", name="itemIcon", depth=3,
                   pos=f"{ix},{iy}", size=f"{sz},{sz}",
                   foregroundlayer="true", atlas="ItemIconAtlas",
                   sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
            if ic.type_icon:
                w.leaf("sprite", name="itemtypeicon", depth=8,
                       pos=f"{ix},{iy}", width=10, height=10,
                       sprite=ic.type_icon, foregroundlayer="true", color="")
        elif n == 2:
            # n=2 vertical stack inset from borders. Sprites have variable
            # built-in padding so use a conservative 11x11 with 4px top/bottom.
            # Container 30 tall: top icon y=-4..-15, bottom icon y=-15..-26.
            # Horizontally: 11 in 26 -> x = 24 + (26-11)/2 = 31 (rounded down).
            cascade = [(31, -4), (31, -15)]
            sz_default = 11
            for i in range(min(n, 2)):
                it = items[i]
                tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
                ic = resolve_item_icon(it)
                ix, iy = cascade[i]
                ov = FOOD_ICON_OVERRIDES.get(ic.sprite) or FOOD_ICON_OVERRIDES.get(it) or {}
                sz = ov.get("size", sz_default)
                ix += ov.get("dx", 0); iy += ov.get("dy", 0)
                w.leaf("sprite", name="itemIcon", depth=3,
                       pos=f"{ix},{iy}", size=f"{sz},{sz}",
                       foregroundlayer="true", atlas="ItemIconAtlas",
                       sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{ix},{iy}", width=8, height=8,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
        else:
            # n>=3: shrink-to-fit grid from _food_compact_layout so extra
            # items (e.g. a game update adding a 3rd item to an existing
            # unlock level) pack into the same small footprint instead of
            # falling back to the much taller generic icon grid.
            for i, it in enumerate(items):
                ix, iy, sz_base = icon_positions[i] if i < len(icon_positions) else (31, -15, 11)
                tip = TOOLTIP_OVERRIDES.get(it) or (it if it in ITEM_ICONS else (names[i] if i < len(names) and names[i] else it))
                ic = resolve_item_icon(it)
                ov = FOOD_ICON_OVERRIDES.get(ic.sprite) or FOOD_ICON_OVERRIDES.get(it) or {}
                sz = ov.get("size", sz_base)
                ix += ov.get("dx", 0); iy += ov.get("dy", 0)
                w.leaf("sprite", name="itemIcon", depth=3,
                       pos=f"{ix},{iy}", size=f"{sz},{sz}",
                       foregroundlayer="true", atlas="ItemIconAtlas",
                       sprite=ic.sprite, color=ic.tint, tooltip_key=tip)
                if ic.type_icon:
                    w.leaf("sprite", name="itemtypeicon", depth=8,
                           pos=f"{ix},{iy}", width=8, height=8,
                           sprite=ic.type_icon, foregroundlayer="true", color="")
        w.close("rect")
        return sec_h
    sec_h = _nontiered_height(n)
    w.open("rect", name=f"Section{idx}", pos=f"0,{y}", width=50, height=sec_h)
    w.leaf("sprite", depth=1, name="backgroundSection", sprite="menu_empty3px",
           width=50, height=sec_h + 2, color="[darkGrey]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=3, name="backgroundBorderSection", sprite="menu_empty3px",
           foregroundlayer="true", width=50, height=sec_h + 2, color="[black]",
           type="sliced", fillcenter="false")
    # Single full-height green fill driven by this level's cvar
    w.leaf("filledsprite", depth=2, name="yesUnlocked", color="75,140,75,255",
           pos="0,0", width=50, height=sec_h, fillcenter="true", type="filled",
           fill=f"{{cvar({sk.name}Check{lvl})}}")
    # One Q1 brown chip
    w.leaf("sprite", depth=2, name="backgroundBorder", sprite="menu_empty3px",
           pos="2,-2", width=21, height=18, color="[black]",
           type="sliced", fillcenter="false")
    w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
           pos="3,-3", width=19, height=16, color=QCOLORS[0],
           type="sliced", fillcenter="true")
    w.leaf("label", depth=3, name="checkunlock", pos="0,-3",
           width=25, height=16, justify="center", text=str(lvl),
           color="[black]", font_size=16)
    _emit_icon_grid(w, items, names)
    w.close("rect")
    return sec_h


def emit_zoomed_view(w: W, sk: Skill, all_skills: list[Skill]) -> None:
    """The full-width zoomed view shown when a skill tab is selected."""
    short_n = sk.short_name
    medical_slots: list[tuple[int, list[str], list[str]]] = []
    medical_mode = (sk.name == "craftingMedical")
    seeds_slots: list[tuple[int, list[str], list[str]]] = []
    seeds_mode = (sk.name == "craftingSeeds")
    food_slots: list[tuple[int, list[str], list[str]]] = []
    food_mode = (sk.name == "craftingFood")
    workstations_slots: list[tuple[int, list[str], list[str]]] = []
    workstations_mode = (sk.name == "craftingWorkstations")
    electrician_mode = (sk.name == "craftingElectrician")
    electrician_by_level: dict[int, tuple[list[str], list[str]]] = {}
    electrician_tiered: DisplayRow | None = None
    medical_cols = 2
    medical_rows = 4
    medical_cell_w = 150
    seeds_cols = 2
    seeds_rows = 5
    seeds_cell_w = 150
    seeds_cell_h = 100
    food_cols = 5
    food_rows = 4
    food_cell_w = 150
    workstations_cols = 4
    workstations_rows = 4
    # Match current per-container width in Forge Ahead's 16/20/26 row.
    workstations_cell_w = 134

    if medical_mode:
        # Medical Journal: flatten to 8 single-slot containers and render as
        # 2 columns x 4 rows with equal width for visual consistency.
        for row in sk.display:
            if row.is_tiered:
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                medical_slots.append((lvl, items, names))
        # Keep exactly 8 visible entries (2x4) in level order.
        medical_slots = medical_slots[:medical_cols * medical_rows]
        cell_width = medical_cell_w
        total_grid_h = medical_rows * 100
        total_grid_w = medical_cols * cell_width
    elif seeds_mode:
        # Southern Farming keeps non-quality semantics: group sections by
        # unlock level, then merge all items unlocked at the same threshold.
        seeds_slots = _group_slots_by_unlock_level(_collect_non_tiered_slots(sk))

        slot_count = max(1, len(seeds_slots))
        seeds_cols = 1 if slot_count <= 2 else 2
        seeds_rows = math.ceil(slot_count / seeds_cols)

        max_icons = max((len(items) for _lvl, items, _names in seeds_slots), default=1)
        # Size Seeds container from the widest rendered row (6-per-row cap)
        # plus left-chip clearance so centered icons do not collide with the
        # unlock chip at x=0..50.
        max_per_row = 6
        if max_icons <= max_per_row:
            widest_row_w = max_icons * 50 + max(0, (max_icons - 1) * 10)
        else:
            widest_row_w = max_per_row * 50 + (max_per_row - 1) * 6
        seeds_cell_w = max(150, widest_row_w + 110)
        if max_icons > max_per_row:
            icon_rows = math.ceil(max_icons / max_per_row)
            # Wiring-55 style baseline for 2 rows (125), then add one row
            # step (50 icon + 8 gap) for each additional row.
            seeds_cell_h = max(125, icon_rows * 58 + 9)
        max_two_col_cell = (WIN_WIDTH - 300) // 2
        if seeds_cols == 2 and seeds_cell_w > max_two_col_cell:
            seeds_cols = 1
            seeds_rows = slot_count
        if seeds_cols == 2:
            seeds_cell_w = min(seeds_cell_w, max_two_col_cell)
        seeds_cell_w = min(seeds_cell_w, WIN_WIDTH - 300)

        cell_width = seeds_cell_w
        total_grid_h = seeds_rows * seeds_cell_h
        total_grid_w = seeds_cols * cell_width
    elif food_mode:
        # Home Cooking Weekly: flatten to 20 single-slot containers and render
        # as 5 columns x 4 rows. Match Seeds container width (150px).
        for row in sk.display:
            if row.is_tiered:
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                food_slots.append((lvl, items, names))
        food_slots = food_slots[:food_cols * food_rows]
        cell_width = food_cell_w
        total_grid_h = food_rows * 100
        total_grid_w = food_cols * cell_width
    elif workstations_mode:
        # Forge Ahead: flatten to N single-slot containers and render as
        # 4 columns x however many rows are needed to fit every unlock slot
        # (grows automatically as new workstation unlocks are added by game
        # updates, instead of silently truncating at a fixed 16). Container
        # width matches the former 3-up row container width used by levels
        # 16/20/26.
        for row in sk.display:
            if row.is_tiered:
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                workstations_slots.append((lvl, items, names))
        workstations_rows = max(1, math.ceil(len(workstations_slots) / workstations_cols))
        cell_width = workstations_cell_w
        total_grid_h = workstations_rows * 100
        total_grid_w = workstations_cols * cell_width
    elif electrician_mode:
        for row in sk.display:
            if row.is_tiered:
                electrician_tiered = row
                continue
            for i, lvl in enumerate(row.unlock_levels):
                items = list(row.level_items[i]) if i < len(row.level_items) else list(row.items)
                if not items:
                    items = list(row.items)
                names = list(row.level_names[i]) if i < len(row.level_names) else list(row.name_keys)
                electrician_by_level[lvl] = (items, names)
        col2_w = 380
        col3_w = 130
        col_gap = 50
        col2_h = 375  # 125 + 125 + 125
        col3_h = 250  # shorter vertical/transposed tiered block
        total_grid_h = max(col2_h, col3_h)
        total_grid_w = col2_w + col_gap + col3_w
        cell_width = col2_w
    else:
        rows_used = min(len(sk.display), 6)
        cell_width = 300  # default; widened automatically when N-up needs more
        # Find widest row -> cell width
        for row in sk.display:
            if not row.is_tiered:
                n = len(row.unlock_levels)
                if n > 0:
                    # Keep 3-way splits aligned with the 300px tiered width.
                    if n <= 3:
                        needed = 300
                    else:
                        needed = max(300, n * 100 + (n - 1) * 2)
                    if needed > cell_width:
                        cell_width = needed
        # Compute total vertical height of the main grid (some rows may be taller
        # than 100px when they use a multi-row icon layout, e.g. Armored Up).
        def _row_h(r):
            return 50 + 50 * len(r.icon_rows) if r.icon_rows else 100
        total_grid_h = sum(_row_h(r) for r in sk.display[:6])
        total_grid_w = cell_width
    # Vertical centering: usable area is y=-110..-720 (height 610, center -415).
    mag_label_y = -225
    title_pos_y = -255
    main_pos_y = -415 + total_grid_h // 2
    # Layout: [magazine 250] [gap 50] [main grid]
    visual_w = 250 + 50 + total_grid_w
    title_pos_x = (WIN_WIDTH - visual_w) // 2
    main_pos_x = title_pos_x + 300

    w.open("rect", name=f"zoomed{short_n}", controller="TabSelectorTab", tab_key="")
    # Magazine name label (sits above the cover sprite). text_key resolves to
    # the localized magazine name from items.xml (e.g. "Tech Planet").
    mag_item = MAG_ITEM.get(sk.name, "")
    if mag_item:
        w.leaf("label", name=f"checklist{short_n}MagName", depth=3,
               pos=f"{title_pos_x},{mag_label_y}", width=250, height=30,
               justify="center", font_size=26, text_key=mag_item)
    # Title block
    w.open("grid", name=f"checklist{short_n}Title", depth=3, rows=2, cols=1,
           pos=f"{title_pos_x},{title_pos_y}", cell_width=250, cell_height=250,
           repeat_content="false", arrangement="horizontal", controller="MapStats")
    w.open("entry", name="Magazine")
    w.leaf("sprite", depth=2, name="background", sprite="menu_empty3px",
           width=250, height=350, color="[black]", type="sliced", fillcenter="true")
    w.leaf("sprite", depth=2, name="backgroundBorder", foregroundlayer="true",
           sprite="menu_empty3px", width=250, height=350, color="[black]",
           type="sliced", fillcenter="false")
    w.leaf("filledsprite", depth=1, name="yesUnlocked", color="0,255,0,200",
           pos="0,0", width=250, height=350, fillcenter="true", type="filled",
           fill=f"{{cvar({sk.name}Check{sk.max_level})}}")
    w.leaf("sprite", depth=3, name="itemIcon", pos="2,-2", width=250, height=250,
           size="246,246", foregroundlayer="true", atlas="ItemIconAtlas",
           sprite=sk.book_sprite, color="", tooltip_key=f"{sk.name}Name")
    w.close("entry")
    w.open("entry", name="level")
    w.leaf("label", depth=3, name="check", pos="0,-14", width=250, height=100,
           justify="center", text=f"[fc8c03]{{cvar({sk.name}Check)}}[-]", font_size=40,
           tooltip_key=f"{sk.name}Name")
    w.leaf("label", depth=3, name="check", pos="0,-50", width=250, height=100,
           justify="center", text=f"/{sk.max_level}", font_size=40,
           tooltip_key=f"{sk.name}Name")
    w.close("entry")
    w.close("grid")
    # Main checklist grid
    if medical_mode:
        w.open("grid", name=f"checklist{short_n}", depth=3,
               rows=medical_rows, cols=medical_cols,
               pos=f"{main_pos_x},{main_pos_y}",
               cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="horizontal",
               controller="MapStats")
        for lvl, items, names in medical_slots:
            emit_qsection_single(w, sk.name, lvl, items, names, cell_width)
        w.close("grid")
    elif seeds_mode:
        w.open("grid", name=f"checklist{short_n}", depth=3,
               rows=seeds_rows, cols=seeds_cols,
               pos=f"{main_pos_x},{main_pos_y}",
               cell_width=cell_width, cell_height=seeds_cell_h,
               repeat_content="false", arrangement="horizontal",
               controller="MapStats")
        for lvl, items, names in seeds_slots:
            emit_qsection_single(w, sk.name, lvl, items, names, cell_width,
                                 section_height=seeds_cell_h)
        w.close("grid")
    elif food_mode:
        w.open("grid", name=f"checklist{short_n}", depth=3,
               rows=food_rows, cols=food_cols,
               pos=f"{main_pos_x},{main_pos_y}",
               cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="horizontal",
               controller="MapStats")
        for lvl, items, names in food_slots:
            emit_qsection_single(w, sk.name, lvl, items, names, cell_width)
        w.close("grid")
    elif workstations_mode:
        w.open("grid", name=f"checklist{short_n}", depth=3,
               rows=workstations_rows, cols=workstations_cols,
               pos=f"{main_pos_x},{main_pos_y}",
               cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="horizontal",
               controller="MapStats")
        for lvl, items, names in workstations_slots:
            emit_qsection_single(w, sk.name, lvl, items, names, cell_width)
        w.close("grid")
    elif electrician_mode:
        col2_w = 380
        col2_h = 375
        col3_w = 130
        col3_h = 220
        col_gap = 50
        col3_x = col2_w + col_gap
        col2_top = 0
        # Independent vertical centering: shorter tier block centered against
        # the taller stacked column (25/45/55).
        col3_top = -((col2_h - col3_h) // 2)
        w.open("rect", name=f"checklist{short_n}", depth=3,
               pos=f"{main_pos_x},{main_pos_y}", width=col2_w + col_gap + col3_w,
               height=col2_h, controller="MapStats")
        items25, names25 = electrician_by_level.get(25, ([], []))
        items45, names45 = electrician_by_level.get(45, ([], []))
        items55, names55 = electrician_by_level.get(55, ([], []))
        _emit_single_section_rect(w, sk.name, 25, items25, names25,
                                  x=0, y=col2_top, width=col2_w, height=125,
                                  row1_count=4)
        _emit_single_section_rect(w, sk.name, 45, items45, names45,
                                  x=0, y=col2_top - 125, width=col2_w, height=125,
                                  row1_count=3)
        _emit_single_section_rect(w, sk.name, 55, items55, names55,
                                  x=0, y=col2_top - 250, width=col2_w, height=125,
                                  row1_count=6, force_cols=6, row2_extra_drop=0)
        if electrician_tiered is not None:
            _emit_tier6_section_vertical_rect(
                w, sk.name,
                electrician_tiered.unlock_levels,
                electrician_tiered.items,
                electrician_tiered.name_keys,
                x=col3_x, y=col3_top, width=col3_w, height=col3_h,
            )
        w.close("rect")
    else:
        w.open("grid", name=f"checklist{short_n}", depth=3, rows=6, cols=3,
               pos=f"{main_pos_x},{main_pos_y}", cell_width=cell_width, cell_height=100,
               repeat_content="false", arrangement="vertical", controller="MapStats")
        for row in sk.display[:6]:
            if row.is_tiered:
                emit_qsection_tier6(w, sk.name, row.unlock_levels, row.items, row.name_keys,
                                    icon_rows=row.icon_rows or None,
                                    icon_row_names=row.icon_row_names or None)
            else:
                emit_qsection_nup(w, sk.name, row.unlock_levels, row.items, row.name_keys, cell_width,
                                  level_items=row.level_items or None,
                                  level_names=row.level_names or None)
        w.close("grid")
    w.close("rect")

# ---------------------------------------------------------------------------
# Tabs shell + window
# ---------------------------------------------------------------------------

def emit_craftinglist_tab(w: W, skills: list[Skill]) -> None:
    by_name = {s.name: s for s in skills}
    w.open("rect", name="craftinglistTab", controller="TabSelectorTab", tab_key="agf1MainTabMagazines")
    # Sub-tab selector with 1 (All) + 23 buttons
    w.open("rect", name="tabs", controller="TabSelector", select_tab_contents_on_open="true")
    # Header. Two stacked elements at the same screen position:
    #   1) tabButtons grid: 24 plain transparent simplebuttons (the click
    #      surfaces; All + 23 skills) using repeat_content="true" template.
    #   2) magazineDecor grid: 23 decorative magazine covers (sprite + level
    #      labels + greenfill) at depth=10 layered on top. NGUI sprites and
    #      labels have no colliders, so clicks pass through to the buttons
    #      below Ã¢â‚¬â€ exactly how the hand-written zoomed views work.
    w.open("rect", name="tabsHeader")
    # ---- (1) Click surfaces ----
    w.open("grid", name="tabButtons", pos="-55,-10", depth=3, rows=1, cols=24,
           cell_width=60, cell_height=100, repeat_content="true", arrangement="horizontal")
    w.open("rect", controller="TabSelectorButton")
    w.leaf("simplebutton", name="tabButton", depth=8, pos="5,0",
           width=50, height=100, font_size=20, bordercolor="[black]",
           defaultcolor="[darkestGrey]", selectedsprite="menu_empty",
           selectedcolor="74, 33, 150", foregroundlayer="false",
           caption="{tab_name_localized}")
    w.close("rect")
    w.close("grid")
    # ---- (2) Decorative magazine overlay ----
    # First cell (col 0) is reserved for the All button (already shows
    # "All" caption from the template above), so the magazine grid starts
    # at column 1: pos x = -55 + 60 = 5.
    # depth=10 (above button at depth=8) + foregroundlayer="true" so they
    # render visually on top of the button background. Sprites and labels
    # have NO NGUI collider, so clicks pass through to the simplebutton.
    # This is exactly the pattern the hand-written zoomed views use.
    w.open("grid", name="magazineDecor", pos="5,-10", depth=10, rows=1, cols=23,
           cell_width=60, cell_height=100, repeat_content="false",
           arrangement="horizontal", controller="MapStats")
    for tab_skill in TAB_ORDER:
        sk = by_name.get(tab_skill)
        if not sk:
            continue
        w.open("entry")
        w.open("rect", depth=10, pos="5,0", width=50, height=100)
        # NOTE: No tooltip_key on these sprites/labels. tooltip_key turns a
        # widget into a hover/click target that blocks the simplebutton
        # underneath. The hand-written zoomed views deliberately omit it.
        w.leaf("sprite", depth=10, name="itemIcon", pos="2,-9", width=50, height=50,
               size="46,46", foregroundlayer="true", atlas="ItemIconAtlas",
               sprite=sk.book_sprite, color="")
        w.leaf("label", depth=10, name="check", pos="0,-60", width=50, height=16,
               justify="center", text=f"[fc8c03]{{cvar({sk.name}Check)}}[-]", font_size=22,
               foregroundlayer="true")
        w.leaf("label", depth=10, name="check", pos="0,-77", width=50, height=16,
               justify="center", text=f"/{sk.max_level}", font_size=22,
               foregroundlayer="true")
        w.leaf("filledsprite", depth=9, name="yesUnlocked", color="10,255,10,65",
               pos="2,-1", width=46, height=98, fillcenter="true", type="filled",
               foregroundlayer="true",
               fill=f"{{cvar({sk.name}Check{sk.max_level})}}")
        w.close("rect")
        w.close("entry")
    w.close("grid")
    w.close("rect")
    # Contents
    w.open("rect", name="tabsContents")
    # All view (compact strips)
    w.open("rect", name="allChecklists", controller="TabSelectorTab", tab_key="lblAll")
    for i, tab_skill in enumerate(TAB_ORDER):
        sk = by_name.get(tab_skill)
        if not sk:
            continue
        x = 10 + i * 60
        emit_compact_strip(w, sk, x)
    w.close("rect")
    # Per-skill zoomed views
    for tab_skill in TAB_ORDER:
        sk = by_name.get(tab_skill)
        if not sk:
            continue
        emit_zoomed_view(w, sk, skills)
    w.close("rect")  # tabsContents
    w.close("rect")  # tabs
    w.close("rect")  # craftinglistTab


def emit_window(
    skills: list[Skill],
    book_series: list[BookSeries] | None = None,
    unlocks: list[UnlockEntry] | None = None,
) -> str:
    w = W()
    w.raw('<?xml version="1.0" encoding="UTF-8"?>')
    w.open("AGF-PurpleBookGenerator")
    # ---- Button on the paging header (this is what opens the window) ----
    w.comment("The Button to access Schematic Checklist")
    w.open("append", xpath="windows/window[@name='windowPagingHeader']")
    w.leaf("sprite", name="background", sprite="ui_game_filled_circle", depth=2,
           pos="-39,-3", height=36, width=36, color="[lightGrey]", type="sliced")
    w.leaf("sprite", name="background", sprite="ui_game_filled_circle", depth=1,
           pos="-41,-1", height=40, width=40, color="0, 0, 0", type="sliced")
    w.leaf("iconbutton", pos="-21,-21", depth="3", selectable="true", collider_scale="1.5",
           name="Schematics", sprite="ui_game_symbol_book_read",
           tooltip_key="agf0PurpleBookButtonTooltip", snap="false", gamepad_selectable="false",
            color_default="221,205,250,255", color_hovered="221,205,250,255",
            color_selected="221,205,250,255", color_disabled="170,170,170,255")
    w.close("append")
    # Reposition the gamepad LB icon to make room for the new button
    w.raw('<set xpath="windows/window[@name=\'windowPagingHeader\']/gamepad_icon[@name=\'LB_Icon\']/@pos">-58,-20</set>')
    # The append into /windows holds the full Schematics window
    w.open("append", xpath="/windows")
    w.open("window", name="Schematics", panel="Left",
           pos="0,0",
           height=WIN_HEIGHT, width=WIN_WIDTH, cursor_area="true",
           globalopacitymod="1.35", controller="MapStats")
    # Non-EnhancedAGF fallback header objects (title/day-time/location)
    # are generated here so manual live-file edits are not required.
    w.open("conditional", evaluator="client")
    w.open("if", cond="mod_loaded('AGF-NoEAC-EnhancedAGF') == false")
    w.open("rect", name="agfPbFallbackTitle", pos="480,102", width="430", height="46", depth="10")
    w.leaf("sprite", name="backgroundDark", sprite="ui_game_header_fill", pos="-3,3",
           width="436", height="52", color="0,0,0,255", type="sliced", pivot="topleft")
    w.leaf("sprite", name="backgroundGrey", sprite="ui_game_header_fill", pos="0,0",
           width="430", height="46", color="96,96,96,255", type="sliced", pivot="topleft",
           globalopacity="false")
    w.leaf("label", name="title", depth="12", text_key="agf0PurpleBookButton", pos="35,-7",
           width="360", height="34", font_size="34", justify="center", style="outline",
           color="[white]", foregroundlayer="true")
    w.close("rect")

    w.open("rect", name="agfPbFallbackDayTime", pos="120,96", width="240", height="60",
           depth="10", controller="MapStats")
    w.leaf("sprite", depth="10", name="timeBackground", sprite="ui_game_header_fill", pos="0,6",
           width="250", height="46", color="0,0,0,225", type="sliced", foregroundlayer="true",
           pivot="topleft")
    w.leaf("sprite", depth="11", name="dayTimeIcon", width="24", height="24", pos="16,-17",
           sprite="ui_game_symbol_clock", color="[iconColor]", foregroundlayer="true", pivot="left",
           visible="{showtime}")
    w.leaf("label", depth="11", name="dayTimeLabel", pos="46,-18", width="188", height="28",
           text="{mapdaytimetitle}: [DECEA3]{mapdaytime}[-]", font_size="20", justify="left",
           pivot="left", visible="{showtime}")
    w.close("rect")

    w.open("rect", name="agfPbFallbackLocation", pos="1010,96", width="240", height="60",
           depth="10", controller="Location")
    w.leaf("sprite", pos="0,6", name="background", sprite="ui_game_header_fill", color="0,0,0,225",
           foregroundlayer="true", height="46", width="250", type="sliced")
    w.leaf("label", depth="11", text="{locationname}", font_size="20", justify="center",
           pos="6,2", height="28")
    w.open("grid", pos="125,-28", pivot="center", cols="8", cell_width="18", cell_height="16")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           color="{difficultycolor1}", visible="{visible1}")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           color="{difficultycolor2}", visible="{visible2}")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           color="{difficultycolor3}", visible="{visible3}")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           color="{difficultycolor4}", visible="{visible4}")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           color="{difficultycolor5}", visible="{visible5}")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           color="{difficultycolor6}", visible="{visible6}")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           color="{difficultycolor7}", visible="{visible7}")
    w.open("rect", width="16", height="16", visible="{visible_half}")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           type="filled", fill="0.5", color="255,128,0")
    w.leaf("sprite", depth="10", name="windowicon", width="16", height="16", sprite="ui_game_symbol_skull",
           type="filled", fill="0.5", fillinvert="true", color="[lightGrey]")
    w.close("rect")
    w.close("grid")
    w.close("rect")
    w.close("if")
    w.close("conditional")

    # Header (just a thin black bar; the page title is owned by windowPagingHeader)
    w.open("rect", name="header")
    w.leaf("sprite", name="background", depth=2, pos="0,43", height=45,
           color="[black]", type="sliced", fillcenter="true")
    w.close("rect")
    # Background: filled center + black border (both must be emitted)
    w.open("rect", name="Background")
    w.leaf("sprite", name="background", depth=2, color="[mediumGrey]",
           type="sliced", fillcenter="true", foregroundlayer="false")
    w.leaf("sprite", name="backgroundBorder", depth=2, sprite="menu_empty3px",
           color="[black]", type="sliced", fillcenter="false",
           on_press="true", foregroundlayer="false")
    w.close("rect")
    # 4-tab shell Ã¢â‚¬â€ header is small + top-centered so it sits above the magazine strip
    w.open("rect", name="tabs", controller="TabSelector", select_tab_contents_on_open="true")
    w.open("rect", name="tabsHeader")
    w.open("grid", name="tabButtons", pos="455,40", depth=2, rows=1, cols=4,
           cell_width=120, cell_height=40, repeat_content="true", arrangement="horizontal")
    w.open("rect", controller="TabSelectorButton")
    w.leaf("simplebutton", name="tabButton", depth=8, width=120, height=40,
           font_size=20, bordercolor="[black]", defaultcolor="[darkGrey]",
           selectedsprite="menu_empty", selectedcolor="74, 33, 150",
           foregroundlayer="false", caption="{tab_name_localized}")
    w.close("rect")
    w.close("grid")
    w.close("rect")  # tabsHeader
    w.open("rect", name="tabsContents")
    emit_craftinglist_tab(w, skills)
    # Placeholder tabs
    for placeholder, key in (
        ("booksTab", "agf1MainTabBooks"),
        ("unlockablesTab", "agf1MainTabUnlocks"),
        ("armorsTab", "agf1MainTabArmors"),
    ):
        w.open("rect", name=placeholder, controller="TabSelectorTab", tab_key=key)
        w.leaf("label", name="placeholder", pos="0,-300", width=WIN_WIDTH, height=40,
               justify="center", text=f"({placeholder} - not yet generated)",
               font_size=24, color="[lightGrey]")
        w.close("rect")
    w.close("rect")  # tabsContents
    w.close("rect")  # tabs
    w.close("window")
    w.close("append")
    w.close("AGF-PurpleBookGenerator")
    return w.render()


def _extract_named_rect(xml_text: str, rect_name: str) -> str | None:
    """Return the full <rect name=...>...</rect> block for rect_name.

    This scanner is resilient to nested <rect> blocks and avoids brittle
    regex-only matching for deep nested XML fragments.
    """
    start_pat = re.compile(rf'<rect\b[^>]*\bname="{re.escape(rect_name)}"[^>]*>', re.IGNORECASE)
    m = start_pat.search(xml_text)
    if not m:
        return None
    i = m.start()
    start_tag = m.group(0)
    if start_tag.rstrip().endswith("/>"):
        return start_tag

    pos = m.end()
    depth = 1
    token_pat = re.compile(r'<rect\b[^>]*?/?>|</rect>', re.IGNORECASE)
    while depth > 0:
        tm = token_pat.search(xml_text, pos)
        if not tm:
            return None
        tok = tm.group(0)
        tok_l = tok.lower()
        if tok_l.startswith("</rect"):
            depth -= 1
        else:
            if not tok.rstrip().endswith("/>"):
                depth += 1
        pos = tm.end()
    return xml_text[i:pos]


def _replace_named_rect(xml_text: str, rect_name: str, replacement_rect: str) -> str:
    """Replace the named rect block with replacement_rect if present."""
    original = _extract_named_rect(xml_text, rect_name)
    if not original:
        return xml_text
    return xml_text.replace(original, replacement_rect, 1)


def _is_placeholder_tab(rect_xml: str | None) -> bool:
    if not rect_xml:
        return True
    return "not yet generated" in rect_xml


def _entry_role(entry_name: str) -> tuple[int, str] | None:
    """Parse entry name variants like 1icon/row1-icon into (row, role)."""
    m = re.match(r'^(?:row)?([1-7])[-_]?((?:icon|description|q1|q6))$', entry_name)
    if not m:
        return None
    return int(m.group(1)), m.group(2)


def _load_compact_armor_example_template() -> dict[str, dict[str, str]]:
    """Load compact armor-card style hints from the hand-edited example file."""
    def _is_disabled_attrs(attrs: dict[str, str] | None) -> bool:
        if not attrs:
            return True
        try:
            return int(attrs.get("width", "1")) <= 0 or int(attrs.get("height", "1")) <= 0
        except ValueError:
            return False

    defaults = {
        "grid": {"cell_height": "22"},
        "row1_description_label": {"pos": "24,-11", "width": "124", "height": "18", "font_size": "16"},
        "row1_q1_label": {"pos": "50,-11", "width": "48", "height": "18", "font_size": "16"},
        "row1_q6_label": {"pos": "38,-11", "width": "48", "height": "18", "font_size": "16"},
        "row1_q1_background": {"pos": "24,0", "width": "52", "height": "22"},
        "row1_q6_background": {"pos": "12,0", "width": "52", "height": "22"},
        "row_body_itemIcon": {"pos": "2,-2", "size": "18,18"},
        "row6_itemIcon": {
            "name": "itemIcon",
            "depth": "9",
            "pos": "2,-12",
            "width": "20",
            "height": "20",
            "foregroundlayer": "true",
            "sprite": "ui_game_symbol_defense",
            "color": "",
            "tooltip_key": ARMOR_SETBONUS_TOOLTIP_KEY,
        },
        "row1_icon_sprites": {
            "backgroundBorderMain": {
                "depth": "7", "name": "backgroundBorderMain", "foregroundlayer": "true", "sprite": "menu_empty3px",
                "pos": "0,0", "width": "256", "height": "168", "color": "[black]", "type": "sliced", "fillcenter": "false",
            },
            "backgroundMain": {
                "depth": "4", "name": "backgroundMain", "sprite": "menu_empty3px", "pos": "0,0", "width": "256", "height": "168",
                "color": "[darkGrey]", "type": "sliced", "fillcenter": "true",
            },
            "cellBorderHeaderRow": {
                "depth": "7", "name": "cellBorderHeaderRow", "foregroundlayer": "true", "sprite": "menu_empty3px", "pos": "0,0",
                "width": "256", "height": "22", "color": "[black]", "fillcenter": "false", "type": "sliced",
            },
            "cellBorderLeftColumnFull": {
                "depth": "7", "name": "cellBorderLeftColumnFull", "foregroundlayer": "true", "sprite": "menu_empty3px", "pos": "0,0",
                "width": "152", "height": "154", "color": "[black]", "fillcenter": "false", "type": "sliced",
            },
            "cellBorderQ1ColumnFull": {
                "depth": "7", "name": "cellBorderQ1ColumnFull", "foregroundlayer": "true", "sprite": "menu_empty3px", "pos": "152,0",
                "width": "52", "height": "154", "color": "[black]", "fillcenter": "false", "type": "sliced",
            },
            "cellBorderQ6ColumnFull": {
                "depth": "7", "name": "cellBorderQ6ColumnFull", "foregroundlayer": "true", "sprite": "menu_empty3px", "pos": "204,0",
                "width": "52", "height": "154", "color": "[black]", "fillcenter": "false", "type": "sliced",
            },
            "headerBackground": {
                "depth": "5", "name": "headerBackground", "pos": "0,0", "width": "256", "height": "22", "color": "[black]",
                "sprite": "menu_empty3px", "type": "sliced", "fillcenter": "true",
            },
        },
        "row2_icon_border": {
            "depth": "8", "name": "cellBorderDescriptionsRow", "foregroundlayer": "true", "sprite": "menu_empty3px", "pos": "0,0",
            "width": "256", "height": "88", "color": "[black]", "fillcenter": "false", "type": "sliced",
        },
        "row6_icon_border": {
            "depth": "8", "name": "cellBorderSetBonusRow", "foregroundlayer": "true", "sprite": "menu_empty3px", "pos": "0,0",
            "width": "256", "height": "44", "color": "[black]", "fillcenter": "false", "type": "sliced",
        },
        "row1_template_sprites": [],
    }

    example_path = DEFAULT_ARMOR_COMPACT_EXAMPLE
    if not example_path.exists():
        return defaults

    try:
        ex_root = ET.parse(example_path).getroot()
    except Exception:
        return defaults

    ex_grid = ex_root.find(".//grid[@rows='7'][@cols='4']")
    if ex_grid is None:
        return defaults

    defaults["grid"]["cell_height"] = ex_grid.attrib.get("cell_height", defaults["grid"]["cell_height"])

    entry_by_name = {e.attrib.get("name", ""): e for e in ex_grid.findall("entry")}

    def _label_attrs(entry_name: str, fallback: dict[str, str]) -> dict[str, str]:
        e = entry_by_name.get(entry_name)
        if e is None:
            return dict(fallback)
        lb = e.find("label")
        if lb is None:
            return dict(fallback)
        out = dict(fallback)
        for k in ("pos", "width", "height", "font_size"):
            if k in lb.attrib:
                out[k] = lb.attrib[k]
        return out

    def _sprite_attrs(entry_name: str, sprite_name: str, fallback: dict[str, str]) -> dict[str, str]:
        e = entry_by_name.get(entry_name)
        if e is None:
            return dict(fallback)
        for sp in e.findall("sprite"):
            if sp.attrib.get("name") == sprite_name:
                out = dict(fallback)
                out.update(sp.attrib)
                out["name"] = sprite_name
                return out
        return dict(fallback)

    defaults["row1_description_label"] = _label_attrs("row1-description", defaults["row1_description_label"])
    defaults["row1_q1_label"] = _label_attrs("row1-q1", defaults["row1_q1_label"])
    defaults["row1_q6_label"] = _label_attrs("row1-q6", defaults["row1_q6_label"])

    defaults["row1_q1_background"] = _sprite_attrs("row1-q1", "q1Background", defaults["row1_q1_background"])
    defaults["row1_q6_background"] = _sprite_attrs("row1-q6", "q6Background", defaults["row1_q6_background"])
    row2_item_icon = _sprite_attrs("row2-icon", "itemIcon", defaults["row_body_itemIcon"])
    body_icon_out = dict(defaults["row_body_itemIcon"])
    for k in ("pos", "size", "width", "height"):
        if k in row2_item_icon:
            body_icon_out[k] = row2_item_icon[k]
    defaults["row_body_itemIcon"] = body_icon_out
    defaults["row2_icon_border"] = _sprite_attrs("row2-icon", "cellBorderDescriptionsRow", defaults["row2_icon_border"])
    defaults["row6_icon_border"] = _sprite_attrs("row6-icon", "cellBorderSetBonusRow", defaults["row6_icon_border"])

    row6_icon = entry_by_name.get("row6-icon")
    if row6_icon is not None:
        for sp in row6_icon.findall("sprite"):
            if sp.attrib.get("name") == "itemIcon":
                defaults["row6_itemIcon"] = dict(sp.attrib)
                break

    # Always use shared localization for set-bonus icon tooltip.
    defaults["row6_itemIcon"]["tooltip_key"] = ARMOR_SETBONUS_TOOLTIP_KEY

    for sprite_name, fallback in list(defaults["row1_icon_sprites"].items()):
        defaults["row1_icon_sprites"][sprite_name] = _sprite_attrs("row1-icon", sprite_name, fallback)

    row1_icon = entry_by_name.get("row1-icon")
    if row1_icon is not None:
        row1_template_sprites: list[dict[str, str]] = []
        for sp in row1_icon.findall("sprite"):
            attrs = dict(sp.attrib)
            if _is_disabled_attrs(attrs):
                continue
            if "name" not in attrs:
                continue
            row1_template_sprites.append(attrs)
        defaults["row1_template_sprites"] = row1_template_sprites

    return defaults


def _apply_compact_armor_template_to_armors_rect(armors_rect_xml: str) -> str:
    """Apply the compact 7x4 armor-card geometry template to all armor grids.

    This mirrors the validated EXAMPLE-ArmorCard-Biker-Compact.xml settings:
    - cell_height=22
    - col widths/offsets: 24 | 128 | 52 | 52 (local x: 0, -40, 24, 12)
    - labels: y=-11, h=18, font_size=16 for 2px top/bottom padding
    - row6 icon merged height: 44 with icon y=-12
    """
    try:
        root = ET.fromstring(f"<root>{armors_rect_xml}</root>")
    except ET.ParseError:
        return armors_rect_xml

    tmpl = _load_compact_armor_example_template()

    def _is_disabled_sprite(attrs: dict[str, str] | None) -> bool:
        if not attrs:
            return True
        try:
            return int(attrs.get("width", "1")) <= 0 or int(attrs.get("height", "1")) <= 0
        except ValueError:
            return False

    def _ensure_sprite(entry: ET.Element, sprite_name: str, attrs: dict[str, str]) -> ET.Element:
        for child in list(entry):
            if child.tag == "sprite" and child.attrib.get("name") == sprite_name:
                child.attrib.update(attrs)
                return child
        sp = ET.Element("sprite", attrs)
        entry.insert(0, sp)
        return sp

    armor_grids: list[ET.Element] = []

    def _set_label_pos_y(lb: ET.Element, y: int) -> None:
        pos = lb.attrib.get("pos", "")
        try:
            x_s, _y_s = pos.split(",", 1)
            lb.set("pos", f"{x_s.strip()},{y}")
        except Exception:
            pass

    def _entry_label(entry_name: str, grid_el: ET.Element) -> ET.Element | None:
        for ent in grid_el.findall("entry"):
            if ent.attrib.get("name", "") != entry_name:
                continue
            for child in list(ent):
                if child.tag == "label":
                    return child
        return None

    def _set_label_y(lb: ET.Element | None, y: int) -> None:
        if lb is None:
            return
        pos = lb.attrib.get("pos", "")
        try:
            x_s, _y_s = pos.split(",", 1)
            lb.set("pos", f"{x_s.strip()},{y}")
        except Exception:
            return

    def _scale_int_pair(val: str, scale: float) -> str:
        try:
            a_s, b_s = val.split(",", 1)
            a = int(round(int(a_s.strip()) * scale))
            b = int(round(int(b_s.strip()) * scale))
            return f"{a},{b}"
        except Exception:
            return val

    def _scale_int_attr(val: str, scale: float) -> str:
        try:
            return str(int(round(int(val.strip()) * scale)))
        except Exception:
            return val

    def _clone_scaled(el: ET.Element, scale: float) -> ET.Element:
        out = ET.Element(el.tag, dict(el.attrib))
        for pair_key in ("pos", "size"):
            if pair_key in out.attrib:
                out.attrib[pair_key] = _scale_int_pair(out.attrib[pair_key], scale)
        for int_key in ("width", "height", "cell_width", "cell_height", "font_size"):
            if int_key in out.attrib:
                out.attrib[int_key] = _scale_int_attr(out.attrib[int_key], scale)
        for ch in list(el):
            out.append(_clone_scaled(ch, scale))
        return out

    def _expand_zoom_quality_columns(grid_el: ET.Element) -> None:
        # Expand zoom cards from q1/q6 to q1..q6 columns (insert q2..q5).
        if grid_el.attrib.get("rows") != "7" or grid_el.attrib.get("cols") != "4":
            return

        grid_name = grid_el.attrib.get("name", "")

        q_label_pos = {1: "88,-19", 2: "67,-19", 3: "46,-19", 4: "25,-19", 5: "4,-19", 6: "-17,-19"}
        q_header_bg_pos = {1: "44,0", 2: "23,0", 3: "2,0", 4: "-19,0", 5: "-40,0", 6: "-61,0"}

        row_entries: list[list[ET.Element]] = []
        for row in range(1, 8):
            icon_e = grid_el.find(f"./entry[@name='{row}icon']")
            desc_e = grid_el.find(f"./entry[@name='{row}description']")
            q1_e = grid_el.find(f"./entry[@name='{row}q1']")
            q6_e = grid_el.find(f"./entry[@name='{row}q6']")
            if icon_e is None or desc_e is None or q1_e is None or q6_e is None:
                return

            q_entries: dict[int, ET.Element] = {1: q1_e, 6: q6_e}

            q1_lb = q1_e.find("label")
            q6_lb = q6_e.find("label")
            q1_text = q1_lb.attrib.get("text", "") if q1_lb is not None else ""
            q6_text = q6_lb.attrib.get("text", "") if q6_lb is not None else ""
            row_values: list[str] | None = None
            if row in (2, 3, 4, 5):
                if grid_name == "armorPrimitiveOutfit":
                    row_values = ["0%", "0%", "0%", "0%", "0%", "0%"]
                else:
                    item_name = _armor_piece_item_name_for_row(grid_name, row)
                    if item_name is not None:
                        row_values = _resolve_armor_row_values_for_display(item_name, q1_text, q6_text)
            elif row in (6, 7):
                row_values = _resolve_setbonus_row_values_for_display(grid_name, row, q1_text, q6_text)

            # Normalize existing q1/q6 geometry for expanded column layout.
            if q1_lb is not None:
                q1_lb.set("pos", q_label_pos[1])
            if q6_lb is not None:
                q6_lb.set("pos", q_label_pos[6])

            if row == 1:
                q1_bg = q1_e.find("./sprite")
                if q1_bg is not None:
                    q1_bg.set("name", "q1Background")
                    q1_bg.set("pos", q_header_bg_pos[1])
                    q1_bg.set("width", "88")
                    q1_bg.set("height", "38")
                    q1_bg.set("color", QCOLORS[0])
                q6_bg = q6_e.find("./sprite")
                if q6_bg is not None:
                    q6_bg.set("name", "q6Background")
                    q6_bg.set("pos", q_header_bg_pos[6])
                    q6_bg.set("width", "88")
                    q6_bg.set("height", "38")
                    q6_bg.set("color", QCOLORS[5])

            for k in (2, 3, 4, 5):
                qk_e = ET.fromstring(ET.tostring(q1_e, encoding="unicode"))
                qk_e.set("name", f"{row}q{k}")

                # Keep row bodies label-only; row1 keeps colored header background.
                for ch in list(qk_e):
                    if ch.tag == "sprite" and row != 1:
                        qk_e.remove(ch)

                qk_lb = qk_e.find("label")
                if qk_lb is None:
                    qk_lb = ET.Element("label")
                    qk_e.append(qk_lb)
                qk_lb.set("pos", q_label_pos[k])

                if row == 1:
                    qk_lb.set("name", f"qualityTier{k}")
                    qk_lb.set("text", str(k))
                    qk_lb.set("color", "[black]")
                    qk_bg = qk_e.find("sprite")
                    if qk_bg is None:
                        qk_bg = ET.Element("sprite")
                        qk_e.insert(0, qk_bg)
                    qk_bg.set("name", f"q{k}Background")
                    qk_bg.set("depth", "6")
                    qk_bg.set("pos", q_header_bg_pos[k])
                    qk_bg.set("width", "88")
                    qk_bg.set("height", "38")
                    qk_bg.set("color", QCOLORS[k - 1])
                    qk_bg.set("sprite", "menu_empty")
                    qk_bg.set("type", "sliced")
                    qk_bg.set("fillcenter", "true")
                else:
                    qk_lb.set("name", f"value{row}q{k}")
                    qk_lb.attrib.pop("text_key", None)
                    if row_values is not None and len(row_values) == 6:
                        qk_lb.set("text", row_values[k - 1])
                    else:
                        qk_lb.set("text", "")

                q_entries[k] = qk_e

            row_entries.append([icon_e, desc_e, q_entries[1], q_entries[2], q_entries[3], q_entries[4], q_entries[5], q_entries[6]])

        # Rebuild entries in expanded order (7 rows x 8 cols).
        for ent in list(grid_el.findall("entry")):
            grid_el.remove(ent)
        for row in row_entries:
            for ent in row:
                grid_el.append(ent)

        grid_el.set("rows", "7")
        grid_el.set("cols", "8")
        grid_el.set("cell_width", "112")
        grid_el.set("cell_height", "38")

        # Expand card/background/dividers to accommodate q2..q5 columns.
        entry1icon = grid_el.find("./entry[@name='1icon']")
        if entry1icon is None:
            return

        def _set_sprite(entry: ET.Element, name: str, attrs: dict[str, str]) -> None:
            sp = entry.find(f"./sprite[@name='{name}']")
            if sp is None:
                merged = {"name": name}
                merged.update(attrs)
                sp = ET.Element("sprite", merged)
                entry.append(sp)
            else:
                sp.attrib.update(attrs)

        _set_sprite(entry1icon, "backgroundMain", {"width": "810", "height": "270"})
        _set_sprite(entry1icon, "borderOuterTop", {"pos": "-2,2", "width": "814", "height": "4"})
        _set_sprite(entry1icon, "borderOuterBottom", {"pos": "-2,-270", "width": "814", "height": "4"})
        _set_sprite(entry1icon, "borderOuterLeft", {"pos": "-2,2", "width": "4", "height": "273"})
        _set_sprite(entry1icon, "borderOuterRight", {"pos": "808,2", "width": "4", "height": "273"})
        _set_sprite(entry1icon, "borderDividerHeader", {"pos": "0,-37", "width": "810", "height": "4"})
        _set_sprite(entry1icon, "borderDividerSetBonus", {"pos": "0,-191", "width": "810", "height": "4"})
        _set_sprite(entry1icon, "borderDividerColDescQ1", {"pos": "264,0", "width": "4", "height": "270"})
        divider_common = {
            "depth": "7",
            "foregroundlayer": "true",
            "sprite": "menu_empty",
            "color": "[black]",
            "globalopacity": "false",
            "fillcenter": "true",
            "type": "sliced",
            "width": "4",
            "height": "270",
        }
        _set_sprite(entry1icon, "borderDividerColQ1Q2", {**divider_common, "pos": "355,0"})
        _set_sprite(entry1icon, "borderDividerColQ2Q3", {**divider_common, "pos": "446,0"})
        _set_sprite(entry1icon, "borderDividerColQ3Q4", {**divider_common, "pos": "537,0"})
        _set_sprite(entry1icon, "borderDividerColQ4Q5", {**divider_common, "pos": "628,0"})
        _set_sprite(entry1icon, "borderDividerColQ5Q6", {**divider_common, "pos": "719,0"})
        _set_sprite(entry1icon, "headerBackground", {"width": "810", "height": "38"})

        # Remove any previously-added merged-description helper sprites.
        for extra_name in (
            "borderDividerDesc2",
            "borderDividerDesc3",
            "borderDividerDesc4",
            "borderDividerDesc5",
            "borderDividerDesc6",
            "descQMask2",
            "descQMask3",
            "descQMask4",
            "descQMask5",
            "descQMask6",
            "descQTop2",
            "descQTop3",
            "descQTop4",
            "descQTop5",
            "descQTop6",
        ):
            extra = entry1icon.find(f"./sprite[@name='{extra_name}']")
            if extra is not None:
                entry1icon.remove(extra)

        old_q1q6 = entry1icon.find("./sprite[@name='borderDividerColQ1Q6']")
        if old_q1q6 is not None:
            entry1icon.remove(old_q1q6)

    # Normalize armors sub-tabs with armors-specific geometry:
    # - keep All outside the main rect
    # - keep tab widths consistent (All matches non-All)
    # - fit non-All tabs inside the rect with equal left/right in-cell gaps
    # - restore content offset used before double-row layout
    tabs_header = root.find(".//rect[@name='tabs']/rect[@name='tabsHeader']")
    tabs_contents = root.find(".//rect[@name='tabs']/rect[@name='tabsContents']")
    if tabs_header is not None:
        tab_grid = None
        all_tab_button = None
        for child in list(tabs_header):
            if child.tag == "grid" and child.attrib.get("name") == "tabButtons" and tab_grid is None:
                tab_grid = child
                continue
            if child.tag == "rect" and child.attrib.get("name") == "allTabButton" and all_tab_button is None:
                all_tab_button = child

        if all_tab_button is None:
            all_tab_button = ET.Element("rect", {"name": "allTabButton", "controller": "TabSelectorButton"})
            all_tab_button.append(
                ET.Element(
                    "simplebutton",
                    {
                        "name": "tabButton",
                        "depth": "8",
                        "pos": "-72,-8",
                        "width": "72",
                        "height": "30",
                        "font_size": "16",
                        "bordercolor": "[black]",
                        "defaultcolor": "[darkestGrey]",
                        "selectedsprite": "menu_empty",
                        "selectedcolor": "74, 33, 150",
                        "foregroundlayer": "false",
                        "caption": "{tab_name_localized}",
                    },
                )
            )
            insert_at = 0
            if tab_grid is not None:
                try:
                    insert_at = list(tabs_header).index(tab_grid)
                except ValueError:
                    insert_at = 0
            tabs_header.insert(insert_at, all_tab_button)
        else:
            all_tab_button.set("controller", "TabSelectorButton")
            btn = all_tab_button.find("simplebutton")
            if btn is None:
                btn = ET.Element("simplebutton")
                all_tab_button.append(btn)
            btn.attrib.update(
                {
                    "name": "tabButton",
                    "depth": "8",
                    "pos": "-72,-8",
                    "width": "72",
                    "height": "30",
                    "font_size": "16",
                    "bordercolor": "[black]",
                    "defaultcolor": "[darkestGrey]",
                    "selectedsprite": "menu_empty",
                    "selectedcolor": "74, 33, 150",
                    "foregroundlayer": "false",
                    "caption": "{tab_name_localized}",
                }
            )

        if tab_grid is not None:
            # 16 non-All tabs in a 1390-wide armors row with equal outer gaps
            # and tighter spacing between tabs:
            # button=78, between=6, cell=84, grid x=20, button x=6
            # -> left edge = right edge = 26.
            tab_grid.attrib.update(
                {
                    "name": "tabButtons",
                    "pos": "20,-10",
                    "depth": "3",
                    "rows": "1",
                    "cell_width": "84",
                    "cell_height": "34",
                    "repeat_content": "true",
                    "arrangement": "horizontal",
                }
            )

            tab_count = 0
            if tabs_contents is not None:
                for tab in list(tabs_contents):
                    if tab.tag != "rect":
                        continue
                    if tab.attrib.get("controller") != "TabSelectorTab":
                        continue
                    if tab.attrib.get("tab_key", ""):
                        tab_count += 1
            non_all_count = max(1, tab_count - 1)
            tab_grid.set("cols", str(non_all_count))

            tab_button_template = tab_grid.find("./rect[@controller='TabSelectorButton']/simplebutton")
            if tab_button_template is not None:
                tab_button_template.attrib.update(
                    {
                        "name": "tabButton",
                        "depth": "8",
                        "pos": "6,2",
                        "width": "78",
                        "height": "30",
                        "font_size": "16",
                        "bordercolor": "[black]",
                        "defaultcolor": "[darkestGrey]",
                        "selectedsprite": "menu_empty",
                        "selectedcolor": "74, 33, 150",
                        "foregroundlayer": "false",
                        "caption": "{tab_name_localized}",
                    }
                )

        if tabs_contents is not None:
            for tab in list(tabs_contents):
                if tab.tag != "rect":
                    continue
                if tab.attrib.get("controller") != "TabSelectorTab":
                    continue
                tab.set("pos", "0,-50")

    for grid in root.findall(".//grid"):
        if grid.attrib.get("rows") != "7" or grid.attrib.get("cols") != "4":
            continue
        gname = grid.attrib.get("name", "")
        if not (gname.startswith("armor") and gname.endswith("Outfit")):
            continue

        armor_grids.append(grid)

        grid.set("cell_height", tmpl["grid"].get("cell_height", "22"))

        for entry in grid.findall("entry"):
            parsed = _entry_role(entry.attrib.get("name", ""))
            if not parsed:
                continue
            row_num, role = parsed

            sprites = [c for c in list(entry) if c.tag == "sprite"]
            labels = [c for c in list(entry) if c.tag == "label"]

            if role == "icon":
                if row_num == 1:
                    row1_template_sprites = tmpl.get("row1_template_sprites", [])
                    if isinstance(row1_template_sprites, list) and row1_template_sprites:
                        for child in list(entry):
                            if child.tag == "sprite":
                                entry.remove(child)
                        for attrs in row1_template_sprites:
                            if _is_disabled_sprite(attrs):
                                continue
                            if "name" not in attrs:
                                continue
                            entry.append(ET.Element("sprite", dict(attrs)))
                        continue

                    for sprite_name in (
                        "backgroundBorderMain",
                        "backgroundMain",
                        "cellBorderHeaderRow",
                        "cellBorderLeftColumnFull",
                        "cellBorderQ1ColumnFull",
                        "cellBorderQ6ColumnFull",
                        "headerBackground",
                    ):
                        attrs = dict(tmpl["row1_icon_sprites"].get(sprite_name, {}))
                        if attrs and not _is_disabled_sprite(attrs):
                            attrs["name"] = sprite_name
                            _ensure_sprite(entry, sprite_name, attrs)
                        elif attrs and _is_disabled_sprite(attrs):
                            for child in list(entry):
                                if child.tag == "sprite" and child.attrib.get("name") == sprite_name:
                                    entry.remove(child)

                for sp in sprites:
                    nm = sp.attrib.get("name", "")
                    if nm in ("cellLine", "cellBorder"):
                        border_attrs: dict[str, str] | None = None
                        if row_num == 1:
                            border_attrs = dict(tmpl["row1_icon_sprites"].get("cellBorderHeaderRow", {}))
                            border_attrs["name"] = "cellBorderHeaderRow"
                        elif row_num == 2:
                            border_attrs = dict(tmpl.get("row2_icon_border", {}))
                            border_attrs["name"] = "cellBorderDescriptionsRow"
                        elif row_num == 6:
                            border_attrs = dict(tmpl.get("row6_icon_border", {}))
                            border_attrs["name"] = "cellBorderSetBonusRow"
                        else:
                            entry.remove(sp)
                        if border_attrs is not None:
                            sp.attrib.update(border_attrs)
                    elif row_num == 2 and nm == "cellBorderDescriptionsRow":
                        border_attrs = dict(tmpl.get("row2_icon_border", {}))
                        if _is_disabled_sprite(border_attrs):
                            entry.remove(sp)
                        else:
                            border_attrs["name"] = "cellBorderDescriptionsRow"
                            sp.attrib.update(border_attrs)
                    elif row_num == 6 and nm == "cellBorderSetBonusRow":
                        border_attrs = dict(tmpl.get("row6_icon_border", {}))
                        if _is_disabled_sprite(border_attrs):
                            entry.remove(sp)
                        else:
                            border_attrs["name"] = "cellBorderSetBonusRow"
                            sp.attrib.update(border_attrs)
                    elif row_num == 1 and nm == "headerBackground":
                        sp.attrib.update(tmpl["row1_icon_sprites"].get("headerBackground", {}))
                        sp.set("name", "headerBackground")
                    elif row_num == 6 and nm == "itemIcon":
                        row6_icon_attrs = dict(tmpl.get("row6_itemIcon", {}))
                        if row6_icon_attrs:
                            row6_icon_attrs.pop("name", None)
                            for k in ("pos", "size", "width", "height", "sprite", "color", "tooltip_key"):
                                if k in row6_icon_attrs:
                                    sp.set(k, row6_icon_attrs[k])
                            if "atlas" in row6_icon_attrs:
                                sp.set("atlas", row6_icon_attrs["atlas"])
                            elif "atlas" in sp.attrib and "sprite" in row6_icon_attrs and row6_icon_attrs.get("sprite", "").startswith("ui_game_symbol_"):
                                sp.attrib.pop("atlas", None)
                    elif row_num in (2, 3, 4, 5) and nm == "itemIcon":
                        body_icon_attrs = dict(tmpl.get("row_body_itemIcon", {}))
                        for k in ("pos", "size", "width", "height"):
                            if k in body_icon_attrs:
                                sp.set(k, body_icon_attrs[k])

            elif role == "description":
                for sp in sprites:
                    nm = sp.attrib.get("name", "")
                    if nm in ("cellLine", "cellBorder"):
                        entry.remove(sp)
                for lb in labels:
                    for k, v in tmpl["row1_description_label"].items():
                        lb.set(k, v)

            elif role == "q1":
                for sp in sprites:
                    nm = sp.attrib.get("name", "")
                    if nm in ("cellLine", "cellBorder"):
                        entry.remove(sp)
                    elif nm == "q1Background":
                        sp.attrib.update(tmpl["row1_q1_background"])
                        sp.set("name", "q1Background")
                for lb in labels:
                    for k, v in tmpl["row1_q1_label"].items():
                        lb.set(k, v)

            elif role == "q6":
                for sp in sprites:
                    nm = sp.attrib.get("name", "")
                    if nm in ("cellLine", "cellBorder"):
                        entry.remove(sp)
                    elif nm == "q6Background":
                        sp.attrib.update(tmpl["row1_q6_background"])
                        sp.set("name", "q6Background")
                for lb in labels:
                    for k, v in tmpl["row1_q6_label"].items():
                        lb.set(k, v)

        # Enforce armor-class header icon by outfit so all overview cards have
        # correct light/medium/heavy visuals before zoom-card propagation.
        type_icon_sprite = ARMOR_TYPE_ICON_BY_OUTFIT.get(gname, "")
        if type_icon_sprite:
            entry_1icon = grid.find("./entry[@name='1icon']")
            if entry_1icon is not None:
                icon_armor = entry_1icon.find("./sprite[@name='iconArmor']")
                if icon_armor is not None:
                    icon_armor.set("sprite", type_icon_sprite)
                    icon_armor.attrib.pop("atlas", None)

        # Armor stat rows use direct vanilla q1..q6 tier values (no interpolation).
        for row in (2, 3, 4, 5):
            item_name = _armor_piece_item_name_for_row(gname, row)
            if item_name is None:
                continue
            lb_q1 = _entry_label(f"{row}q1", grid)
            lb_q6 = _entry_label(f"{row}q6", grid)
            if lb_q1 is None or lb_q6 is None:
                continue
            resolved = _resolve_armor_row_values_for_display(
                item_name,
                lb_q1.attrib.get("text", ""),
                lb_q6.attrib.get("text", ""),
            )
            if resolved is None or len(resolved) != 6:
                continue
            lb_q1.set("text", resolved[0])
            lb_q6.set("text", resolved[5])

        # Primitive armor has no crit-resist bonuses; show explicit zeros.
        if gname == "armorPrimitiveOutfit":
            for row in (2, 3, 4, 5):
                lb_q1 = _entry_label(f"{row}q1", grid)
                lb_q6 = _entry_label(f"{row}q6", grid)
                if lb_q1 is not None:
                    lb_q1.set("text", "0%")
                if lb_q6 is not None:
                    lb_q6.set("text", "0%")

        # Per-armor row6 value overrides for compact cards.
        if gname in ARMOR_SETBONUS_VALUE_OVERRIDES:
            set_q1, set_q6 = ARMOR_SETBONUS_VALUE_OVERRIDES[gname]
            lb_6q1 = _entry_label("6q1", grid)
            lb_6q6 = _entry_label("6q6", grid)
            if lb_6q1 is not None:
                lb_6q1.set("text", set_q1)
            if lb_6q6 is not None:
                lb_6q6.set("text", set_q6)

        if gname in ARMOR_SETBONUS_ROW_VALUE_OVERRIDES:
            for entry_name, value in ARMOR_SETBONUS_ROW_VALUE_OVERRIDES[gname].items():
                lb = _entry_label(entry_name, grid)
                if lb is not None:
                    lb.set("text", value)

        # Resolve set-bonus row edge values from source tiers (no interpolation).
        for row in (6, 7):
            lb_q1 = _entry_label(f"{row}q1", grid)
            lb_q6 = _entry_label(f"{row}q6", grid)
            if lb_q1 is None or lb_q6 is None:
                continue
            resolved = _resolve_setbonus_row_values_for_display(
                gname,
                row,
                lb_q1.attrib.get("text", ""),
                lb_q6.attrib.get("text", ""),
            )
            if resolved is None or len(resolved) != 6:
                continue
            lb_q1.set("text", resolved[0])
            lb_q6.set("text", resolved[5])

        # Route set-bonus text through localization keys so values can be
        # maintained in Localization.txt.
        base = gname[len("armor"):-len("Outfit")]
        setbonus1_key = f"agfArmor{base}SetBonus1"
        setbonus2_key = f"agfArmor{base}SetBonus2"

        # If row7 bonus is absent (or explicitly "none"), center row6 text/values
        # within the combined set-bonus area (rows 6+7).
        lb_7desc = _entry_label("7description", grid)
        lb_7q1 = _entry_label("7q1", grid)
        lb_7q6 = _entry_label("7q6", grid)
        single_bonus = gname not in ARMOR_FORCE_DUAL_SETBONUS
        row7_values_empty = True
        if lb_7q1 is not None and lb_7q1.attrib.get("text", "").strip():
            row7_values_empty = False
        if lb_7q6 is not None and lb_7q6.attrib.get("text", "").strip():
            row7_values_empty = False
        if lb_7desc is not None:
            row7_text = lb_7desc.attrib.get("text", "").strip()
            row7_key = lb_7desc.attrib.get("text_key", "").strip()
            if row7_text or row7_key:
                if row7_text.lower() != "none" and row7_key.lower() != "none":
                    single_bonus = False
            if row7_values_empty:
                single_bonus = True

        if single_bonus:
            for entry_name in ("6description", "6q1", "6q6"):
                lb = _entry_label(entry_name, grid)
                if lb is not None:
                    _set_label_pos_y(lb, -22)
            if lb_7desc is not None:
                lb_7desc.set("text", "")
                lb_7desc.attrib.pop("text_key", None)
            if lb_7q1 is not None:
                lb_7q1.set("text", "")
            if lb_7q6 is not None:
                lb_7q6.set("text", "")
        else:
            for entry_name in ("6description", "6q1", "6q6"):
                lb = _entry_label(entry_name, grid)
                if lb is not None:
                    _set_label_pos_y(lb, -11)

        lb_6desc = _entry_label("6description", grid)
        if lb_6desc is not None:
            lb_6desc.attrib.pop("text", None)
            lb_6desc.set("text_key", setbonus1_key)

        if lb_7desc is not None:
            lb_7desc.attrib.pop("text", None)
            if single_bonus:
                # Keep row7 visually empty for single-bonus cards.
                lb_7desc.attrib.pop("text_key", None)
            else:
                lb_7desc.set("text_key", setbonus2_key)

    # Left armor rating column horizontal gutter normalization.
    for g in root.findall(".//grid"):
        if g.attrib.get("name", "") not in ("lightarmor", "mediumarmor", "heavyarmor"):
            continue
        if g.attrib.get("rows") != "7" or g.attrib.get("cols") != "2":
            continue
        if g.attrib.get("cell_width") != "100":
            continue
        pos = g.attrib.get("pos", "")
        try:
            _x, y_s = pos.split(",", 1)
            g.set("pos", f"37,{int(y_s.strip())}")
        except Exception:
            continue

    # Non-overview armor tabs use the large single rating panel (400x402).
    # Vertically center this panel within the fixed tab content area so the
    # top and bottom gaps are balanced.
    tabs_contents = root.find(".//rect[@name='tabs']/rect[@name='tabsContents']")
    if tabs_contents is not None:
        non_overview_area_top = 0
        non_overview_area_bottom = -671
        rating_panel_h = 402
        available_h = non_overview_area_top - non_overview_area_bottom
        centered_y = non_overview_area_top - max(0, (available_h - rating_panel_h) // 2)
        zoom_card_target_y = -2

        def _ensure_zoom_description_panel(tab_el: ET.Element, zoom_grid_el: ET.Element) -> None:
            gname = zoom_grid_el.attrib.get("name", "")
            if not (gname.startswith("armor") and gname.endswith("Outfit")):
                return

            panel_name = f"{gname}DescPanel"
            existing_panel = tab_el.find(f"./rect[@name='{panel_name}']")
            if existing_panel is not None:
                tab_el.remove(existing_panel)

            try:
                gx_s, gy_s = zoom_grid_el.attrib.get("pos", "0,0").split(",", 1)
                card_x = int(gx_s.strip())
                card_y = int(gy_s.strip())
            except Exception:
                return

            bg = zoom_grid_el.find("./entry[@name='1icon']/sprite[@name='backgroundMain']")
            try:
                card_w = int(bg.attrib.get("width", "810")) if bg is not None else 810
            except Exception:
                card_w = 810
            try:
                card_h = int(bg.attrib.get("height", "270")) if bg is not None else 270
            except Exception:
                card_h = 270

            base = gname[len("armor"):-len("Outfit")]
            rows = [
                (2, f"agfArmor{base}HelmetDescription"),
                (3, f"agfArmor{base}OutfitDescription"),
                (4, f"agfArmor{base}GloveDescription"),
                (5, f"agfArmor{base}BootsDescription"),
                (6, f"agfArmor{base}SetBonusDesc"),
            ]

            section_h = 60
            section_gap = 0
            panel_h = section_h * len(rows) + section_gap * (len(rows) - 1)
            # Fit card + desc panel inside the tab content area with near-even
            # top / between / bottom gaps.
            content_h = 671  # y-range 0..-671
            free_h = max(0, content_h - card_h - panel_h)
            base_gap = free_h // 3
            rem = free_h - (base_gap * 3)
            top_gap = base_gap + (1 if rem > 0 else 0)
            between_gap = base_gap + (1 if rem > 1 else 0)
            card_y = -top_gap
            zoom_grid_el.set("pos", f"{card_x},{card_y}")
            panel_y = card_y - card_h - between_gap
            icon_col_w = ZOOM_ARMOR_DESC_ICON_COL_W
            desc_col_w = max(180, card_w - icon_col_w)
            border_px = 3
            border_depth = "7"

            panel_attrs = {
                "name": panel_name,
                "depth": zoom_grid_el.attrib.get("depth", "7"),
                "pos": f"{card_x},{panel_y}",
                "width": str(card_w),
                "height": str(panel_h),
            }
            panel = ET.Element("rect", panel_attrs)

            # Single connected card container for all five rows.
            panel.append(
                ET.Element(
                    "sprite",
                    {
                        "name": "panelBackground",
                        "depth": "5",
                        "pos": "0,0",
                        "width": str(card_w),
                        "height": str(panel_h),
                        "color": "70,70,70",
                        "sprite": "menu_empty",
                        "type": "sliced",
                        "fillcenter": "true",
                    },
                )
            )
            panel.append(
                ET.Element(
                    "sprite",
                    {
                        "name": "borderOuterTop",
                        "depth": border_depth,
                        "foregroundlayer": "true",
                        "sprite": "menu_empty",
                        "pos": "-2,2",
                        "width": str(card_w + 4),
                        "height": "4",
                        "color": "[black]",
                        "globalopacity": "false",
                        "fillcenter": "true",
                        "type": "sliced",
                    },
                )
            )
            panel.append(
                ET.Element(
                    "sprite",
                    {
                        "name": "borderOuterBottom",
                        "depth": border_depth,
                        "foregroundlayer": "true",
                        "sprite": "menu_empty",
                        "pos": f"-2,-{panel_h}",
                        "width": str(card_w + 4),
                        "height": "4",
                        "color": "[black]",
                        "globalopacity": "false",
                        "fillcenter": "true",
                        "type": "sliced",
                    },
                )
            )
            panel.append(
                ET.Element(
                    "sprite",
                    {
                        "name": "borderOuterLeft",
                        "depth": border_depth,
                        "foregroundlayer": "true",
                        "sprite": "menu_empty",
                        "pos": "-2,2",
                        "width": "4",
                        "height": str(panel_h + 3),
                        "color": "[black]",
                        "globalopacity": "false",
                        "fillcenter": "true",
                        "type": "sliced",
                    },
                )
            )
            panel.append(
                ET.Element(
                    "sprite",
                    {
                        "name": "borderOuterRight",
                        "depth": border_depth,
                        "foregroundlayer": "true",
                        "sprite": "menu_empty",
                        "pos": f"{card_w - 2},2",
                        "width": "4",
                        "height": str(panel_h + 3),
                        "color": "[black]",
                        "globalopacity": "false",
                        "fillcenter": "true",
                        "type": "sliced",
                    },
                )
            )

            for idx, (row_num, desc_key) in enumerate(rows):
                row_y = -(idx * (section_h + section_gap))
                section = ET.Element(
                    "rect",
                    {
                        "name": f"section{row_num}",
                        "depth": str(int(panel_attrs["depth"]) + 1 if panel_attrs["depth"].isdigit() else 8),
                        "pos": f"0,{row_y}",
                        "width": str(card_w),
                        "height": str(section_h),
                    },
                )
                if idx < len(rows) - 1:
                    section.append(
                        ET.Element(
                            "sprite",
                            {
                                "name": "rowSeparator",
                                "depth": "8",
                                "foregroundlayer": "true",
                                "sprite": "menu_empty",
                                "pos": f"0,-{section_h - 1}",
                                "width": str(card_w),
                                "height": str(border_px),
                                "color": "[black]",
                                "globalopacity": "false",
                                "fillcenter": "true",
                                "type": "sliced",
                            },
                        )
                    )
                # Single divider: icon | merged description.
                section.append(
                    ET.Element(
                        "sprite",
                        {
                            "name": "dividerIconStat",
                            "depth": "7",
                            "foregroundlayer": "true",
                            "sprite": "menu_empty",
                            "pos": f"{icon_col_w},0",
                            "width": "3",
                            "height": str(section_h),
                            "color": "[black]",
                            "globalopacity": "false",
                            "fillcenter": "true",
                            "type": "sliced",
                        },
                    )
                )

                src_icon = zoom_grid_el.find(f"./entry[@name='{row_num}icon']/sprite[@name='itemIcon']")
                icon_size = 32
                icon_x = max(0, (icon_col_w - icon_size) // 2)
                icon_y = -max(0, (section_h - icon_size) // 2)
                icon_attrs = {
                    "name": f"icon{row_num}",
                    "depth": "8",
                    "pos": f"{icon_x},{icon_y}",
                    "size": f"{icon_size},{icon_size}",
                    "foregroundlayer": "true",
                    "sprite": "ui_game_symbol_defense",
                    "color": "228,228,228",
                }
                if src_icon is not None:
                    if "atlas" in src_icon.attrib:
                        icon_attrs["atlas"] = src_icon.attrib["atlas"]
                    if "sprite" in src_icon.attrib:
                        icon_attrs["sprite"] = src_icon.attrib["sprite"]
                    if "color" in src_icon.attrib:
                        icon_attrs["color"] = src_icon.attrib["color"]
                    if "tooltip_key" in src_icon.attrib:
                        icon_attrs["tooltip_key"] = src_icon.attrib["tooltip_key"]
                section.append(ET.Element("sprite", icon_attrs))

                desc_label = ET.Element(
                    "label",
                    {
                        "name": f"descLabel{row_num}",
                        "depth": "8",
                        "pos": f"{icon_col_w + 12},-{section_h // 2}",
                        "width": str(max(180, desc_col_w - ZOOM_ARMOR_DESC_LABEL_MARGIN_PX)),
                        "height": str(max(32, section_h - 6)),
                        "justify": "left",
                        "pivot": "left",
                        "font_size": str(ZOOM_ARMOR_DESC_FONT_SIZE),
                        "wrap": "true",
                        "multiline": "true",
                        "color": "214,214,214",
                        "text_key": desc_key,
                    },
                )

                section.append(desc_label)
                panel.append(section)

            tab_el.append(panel)

        overview_tab = tabs_contents.find("./rect[@name='allArmors']")

        for tab in list(tabs_contents):
            if tab.tag != "rect":
                continue
            if tab.attrib.get("controller") != "TabSelectorTab":
                continue
            # Hard guard: never transform overview container.
            if tab.attrib.get("name", "") == "allArmors":
                continue
            if tab.attrib.get("tab_key", "") == "agf1MainTabOverview":
                continue

            for g in tab.findall("grid"):
                if g.attrib.get("name", "") not in ("lightarmor", "mediumarmor", "heavyarmor"):
                    continue
                if g.attrib.get("rows") != "7" or g.attrib.get("cols") != "2":
                    continue
                if g.attrib.get("cell_width") != "200":
                    continue
                pos = g.attrib.get("pos", "")
                try:
                    x_s, _y_s = pos.split(",", 1)
                    g.set("pos", f"{x_s.strip()},{centered_y}")
                except Exception:
                    continue

            # Stage-1 zoomed card generation rule:
            # Start from overview card generation and evenly scale everything.
            # (Columns 2..5 will be added in a separate stage.)
            overview_tab = tabs_contents.find("./rect[@name='allArmors']")
            if overview_tab is not None:
                for old_grid in list(tab.findall("grid")):
                    gname = old_grid.attrib.get("name", "")
                    if not (gname.startswith("armor") and gname.endswith("Outfit")):
                        continue
                    ov_grid = overview_tab.find(f"./grid[@name='{gname}']")
                    if ov_grid is None:
                        continue

                    # Scale from overview using a fixed zoom text target.
                    target_font_size = 28
                    source_font_size = 16
                    ov_label = ov_grid.find("./entry/label[@font_size]")
                    if ov_label is None:
                        ov_label = ov_grid.find(".//label[@font_size]")
                    if ov_label is not None:
                        try:
                            source_font_size = int(ov_label.attrib.get("font_size", "16"))
                        except Exception:
                            source_font_size = 16

                    scale = float(target_font_size) / float(max(1, source_font_size))
                    scale = max(1.0, min(scale, 4.0))

                    new_grid = _clone_scaled(ov_grid, scale)
                    _expand_zoom_quality_columns(new_grid)

                    # Set-bonus positioning rules (zoom cards):
                    # - icon6 always fixed at 5,-29
                    # - single bonus: row6 text/value at y=-29 (row7 empty)
                    # - dual bonus: row6+row7 text/value lowered to y=-22
                    new_sb_icon = new_grid.find("./entry[@name='6icon']/sprite[@name='itemIcon']")
                    if new_sb_icon is not None:
                        new_sb_icon.set("pos", "5,-29")

                    new_row7_desc = _entry_label("7description", new_grid)
                    new_row7_q1 = _entry_label("7q1", new_grid)
                    new_row7_q2 = _entry_label("7q2", new_grid)
                    new_row7_q3 = _entry_label("7q3", new_grid)
                    new_row7_q4 = _entry_label("7q4", new_grid)
                    new_row7_q5 = _entry_label("7q5", new_grid)
                    new_row7_q6 = _entry_label("7q6", new_grid)
                    new_row6_desc = _entry_label("6description", new_grid)
                    new_row6_q1 = _entry_label("6q1", new_grid)
                    new_row6_q2 = _entry_label("6q2", new_grid)
                    new_row6_q3 = _entry_label("6q3", new_grid)
                    new_row6_q4 = _entry_label("6q4", new_grid)
                    new_row6_q5 = _entry_label("6q5", new_grid)
                    new_row6_q6 = _entry_label("6q6", new_grid)
                    single_bonus = True
                    if new_row7_desc is not None:
                        r7_text = (new_row7_desc.attrib.get("text", "") or "").strip().lower()
                        r7_key = (new_row7_desc.attrib.get("text_key", "") or "").strip().lower()
                        if (r7_text and r7_text != "none") or (r7_key and r7_key != "none"):
                            single_bonus = False
                    if new_row7_q1 is not None and (new_row7_q1.attrib.get("text", "") or "").strip():
                        single_bonus = False
                    if new_row7_q6 is not None and (new_row7_q6.attrib.get("text", "") or "").strip():
                        single_bonus = False
                    if single_bonus:
                        for lb in (new_row6_desc, new_row6_q1, new_row6_q2, new_row6_q3, new_row6_q4, new_row6_q5, new_row6_q6):
                            _set_label_y(lb, -41)
                    else:
                        for lb in (
                            new_row6_desc,
                            new_row6_q1,
                            new_row6_q2,
                            new_row6_q3,
                            new_row6_q4,
                            new_row6_q5,
                            new_row6_q6,
                            new_row7_desc,
                            new_row7_q1,
                            new_row7_q2,
                            new_row7_q3,
                            new_row7_q4,
                            new_row7_q5,
                            new_row7_q6,
                        ):
                            _set_label_y(lb, -22)

                    # Keep per-tab placement and z-layer intent.
                    if "pos" in old_grid.attrib:
                        try:
                            ox_s, oy_s = old_grid.attrib["pos"].split(",", 1)
                            new_grid.set("pos", old_grid.attrib["pos"])
                        except Exception:
                            new_grid.set("pos", old_grid.attrib["pos"])
                    if "depth" in old_grid.attrib:
                        new_grid.set("depth", old_grid.attrib["depth"])

                    tab.remove(old_grid)
                    tab.append(new_grid)

            # Horizontal centering (zoom tabs only):
            # keep equal spacing between left edge -> rating panel -> armor card -> right edge.
            rating_grids: list[ET.Element] = []
            for rg in tab.findall("grid"):
                if rg.attrib.get("name", "") in ("lightarmor", "mediumarmor", "heavyarmor"):
                    rating_grids.append(rg)

            zoom_grid = None
            for zg in tab.findall("grid"):
                zn = zg.attrib.get("name", "")
                if zn.startswith("armor") and zn.endswith("Outfit"):
                    zoom_grid = zg
                    break

            if rating_grids and zoom_grid is not None:
                try:
                    rating_w = int(rating_grids[0].attrib.get("cols", "2")) * int(rating_grids[0].attrib.get("cell_width", "200"))
                except Exception:
                    rating_w = 400

                card_w = 810
                bg = zoom_grid.find("./entry[@name='1icon']/sprite[@name='backgroundMain']")
                if bg is not None:
                    try:
                        card_w = int(bg.attrib.get("width", "810"))
                    except Exception:
                        card_w = 810

                container_w = 1390
                if rating_w + card_w < container_w:
                    gap = max(0, (container_w - rating_w - card_w) // 3)
                    rating_x = gap
                    card_x = rating_x + rating_w + gap

                    for rg in rating_grids:
                        pos = rg.attrib.get("pos", "")
                        try:
                            _x_s, y_s = pos.split(",", 1)
                            rg.set("pos", f"{rating_x},{y_s.strip()}")
                        except Exception:
                            pass

                    zpos = zoom_grid.attrib.get("pos", "")
                    try:
                        _x_s, _y_s = zpos.split(",", 1)
                        zoom_grid.set("pos", f"{card_x},{zoom_card_target_y}")
                    except Exception:
                        pass

            if zoom_grid is not None:
                zpos = zoom_grid.attrib.get("pos", "")
                try:
                    zx_s, _zy_s = zpos.split(",", 1)
                    zoom_grid.set("pos", f"{zx_s.strip()},{zoom_card_target_y}")
                except Exception:
                    pass
                _ensure_zoom_description_panel(tab, zoom_grid)

            # Normalize zoomed outfit card semantics to stay aligned with
            # overview rules while keeping the zoomed 1..6 quality layout.
            for zg in tab.findall("grid"):
                zg_name = zg.attrib.get("name", "")
                if not (zg_name.startswith("armor") and zg_name.endswith("Outfit")):
                    continue
                if zg.attrib.get("rows") != "6" or zg.attrib.get("cols") != "12":
                    continue

                overview_grid = None
                if overview_tab is not None:
                    overview_grid = overview_tab.find(f"./grid[@name='{zg_name}']")

                # Title and header icon must mirror the corresponding overview card.
                if overview_grid is not None:
                    ov_title = overview_grid.find("./entry[@name='1description']/label")
                    z_title = zg.find(".//label[@name='TypeArmor']")
                    if ov_title is not None and z_title is not None:
                        z_title.attrib.pop("text", None)
                        z_title.attrib.pop("text_key", None)
                        ov_text = (ov_title.attrib.get("text", "") or "").strip()
                        ov_key = (ov_title.attrib.get("text_key", "") or "").strip()
                        if ov_text:
                            z_title.set("text", ov_text)
                        elif ov_key:
                            z_title.set("text_key", ov_key)
                        if "color" in ov_title.attrib:
                            z_title.set("color", ov_title.attrib["color"])

                    ov_header_icon = overview_grid.find("./entry[@name='1icon']/sprite[@name='iconArmor']")
                    z_header_icon = zg.find(".//sprite[@name='iconArmor']")
                    if ov_header_icon is not None and z_header_icon is not None:
                        if "sprite" in ov_header_icon.attrib:
                            z_header_icon.set("sprite", ov_header_icon.attrib["sprite"])
                        if "color" in ov_header_icon.attrib:
                            z_header_icon.set("color", ov_header_icon.attrib["color"])

                    # Copy overview stat row localization and edge values (Q1/Q6)
                    # into zoomed rows so rules/values stay in lockstep.
                    z_entries = list(zg.findall("entry"))

                    def _copy_label_text_or_key(src: ET.Element | None, dst: ET.Element | None) -> None:
                        if src is None or dst is None:
                            return
                        dst.attrib.pop("text", None)
                        dst.attrib.pop("text_key", None)
                        s_text = (src.attrib.get("text", "") or "").strip()
                        s_key = (src.attrib.get("text_key", "") or "").strip()
                        if s_text:
                            dst.set("text", s_text)
                        elif s_key:
                            dst.set("text_key", s_key)

                    # row blocks in zoomed 6x12 grid:
                    # header=0..11, stat rows start at 12 and stride 12, setbonus starts at 60.
                    for row_num in (2, 3, 4, 5):
                        src_desc = _entry_label(f"{row_num}description", overview_grid)
                        src_q1 = _entry_label(f"{row_num}q1", overview_grid)
                        src_q6 = _entry_label(f"{row_num}q6", overview_grid)

                        block = 12 + (row_num - 2) * 12
                        if len(z_entries) >= block + 12:
                            dst_desc = z_entries[block + 1].find("label")
                            _copy_label_text_or_key(src_desc, dst_desc)

                            dst_q1 = z_entries[block + 6].find("label")
                            dst_q6 = z_entries[block + 11].find("label")
                            _copy_label_text_or_key(src_q1, dst_q1)
                            _copy_label_text_or_key(src_q6, dst_q6)

                    # Set-bonus row uses overview row6 localization/edge values.
                    src_sb_desc = _entry_label("6description", overview_grid)
                    src_sb_q1 = _entry_label("6q1", overview_grid)
                    src_sb_q6 = _entry_label("6q6", overview_grid)
                    sb_block = 60
                    if len(z_entries) >= sb_block + 12:
                        # In zoomed layout, SetBonus description label lives on
                        # the SetBonus entry itself (block start), not +1.
                        dst_sb_desc = z_entries[sb_block].find("label")
                        _copy_label_text_or_key(src_sb_desc, dst_sb_desc)

                        dst_sb_q1 = z_entries[sb_block + 6].find("label")
                        dst_sb_q6 = z_entries[sb_block + 11].find("label")
                        _copy_label_text_or_key(src_sb_q1, dst_sb_q1)
                        _copy_label_text_or_key(src_sb_q6, dst_sb_q6)

                # Ensure top quality headers are explicitly 1..6 with canonical colors.
                quality_labels = zg.findall("./entry/label[@name='qualityTier']")
                for i, qlb in enumerate(quality_labels[:6]):
                    q_index = i + 1
                    qlb.set("text", str(q_index))
                    qlb.set("color", "[black]")
                    entry = qlb.getparent() if hasattr(qlb, "getparent") else None
                    # xml.etree has no getparent; resolve via scan.
                    if entry is None:
                        for ent in zg.findall("entry"):
                            if qlb in list(ent):
                                entry = ent
                                break
                    if entry is None:
                        continue
                    bg = entry.find("sprite[@name='background']")
                    if bg is not None:
                        bg.set("color", QCOLORS[i])
                    bdr = entry.find("sprite[@name='backgroundBorder']")
                    if bdr is not None:
                        bdr.set("color", "[black]")

                # Set set-bonus symbol tooltip to dedicated localization key
                # and ensure zoomed row includes the same defense symbol concept
                # as overview cards.
                for sp in zg.findall(".//sprite[@name='itemIcon']"):
                    spr = sp.attrib.get("sprite", "")
                    if spr.startswith("ui_game_symbol_"):
                        sp.set("tooltip_key", ARMOR_SETBONUS_TOOLTIP_KEY)
                        sp.attrib.pop("atlas", None)

                setbonus_entry = zg.find("./entry[@name='SetBonus']")
                if setbonus_entry is not None:
                    sb_sprite = setbonus_entry.find("./sprite[@name='itemIcon']")
                    if sb_sprite is None:
                        sb_sprite = ET.Element("sprite", {"name": "itemIcon"})
                        setbonus_entry.insert(0, sb_sprite)

                    ov_setbonus_icon = None
                    if overview_grid is not None:
                        ov_setbonus_icon = overview_grid.find("./entry[@name='6icon']/sprite[@name='itemIcon']")

                    symbol_sprite = "ui_game_symbol_defense"
                    symbol_color = "228,228,228"
                    if ov_setbonus_icon is not None:
                        symbol_sprite = ov_setbonus_icon.attrib.get("sprite", symbol_sprite)
                        symbol_color = ov_setbonus_icon.attrib.get("color", symbol_color)

                    sb_sprite.attrib.update(
                        {
                            "depth": "7",
                            "name": "itemIcon",
                            "pos": "7,0",
                            "size": "36,36",
                            "foregroundlayer": "true",
                            "sprite": symbol_sprite,
                            "color": symbol_color,
                            "tooltip_key": ARMOR_SETBONUS_TOOLTIP_KEY,
                        }
                    )
                    sb_sprite.attrib.pop("atlas", None)

    return "".join(ET.tostring(child, encoding="unicode") for child in list(root))


def _collect_armors_setbonus_localization(armors_rect_xml: str) -> dict[str, str]:
    """Create starter localization rows for armor set bonus 1/2 keys."""
    try:
        root = ET.fromstring(f"<root>{armors_rect_xml}</root>")
    except ET.ParseError:
        return {}

    out: dict[str, str] = {}

    def _entry_label_text(grid_el: ET.Element, entry_name: str) -> str:
        for ent in grid_el.findall("entry"):
            if ent.attrib.get("name", "") != entry_name:
                continue
            for child in list(ent):
                if child.tag != "label":
                    continue
                txt = (child.attrib.get("text", "") or "").strip()
                if txt:
                    return txt
                key = (child.attrib.get("text_key", "") or "").strip()
                if key:
                    return key
        return ""

    for grid in root.findall(".//grid"):
        gname = grid.attrib.get("name", "")
        if not (gname.startswith("armor") and gname.endswith("Outfit")):
            continue
        if grid.attrib.get("rows") != "7" or grid.attrib.get("cols") != "4":
            continue
        base = gname[len("armor"):-len("Outfit")]
        key1 = f"agfArmor{base}SetBonus1"
        key2 = f"agfArmor{base}SetBonus2"
        txt1 = _entry_label_text(grid, "6description") or "none"
        txt2 = _entry_label_text(grid, "7description") or "none"
        out[key1] = txt1
        out[key2] = txt2

    return out


def _validate_armors_rect_zoomed_cards(armors_rect_xml: str) -> tuple[bool, list[str]]:
    """Validate zoomed armor cards in armorsTab (non-overview tabs).

        Required invariants:
        - Each non-overview tab has one armor outfit grid.
        - Zoomed armor grid shape is either:
            a) rows=6, cols=12 with quality headers 1..6, or
            b) rows=7, cols=4 with quality headers q1/q6 (overview-style), or
            c) rows=7, cols=8 with quality headers q1..q6.
    """
    issues: list[str] = []
    try:
        root = ET.fromstring(f"<root>{armors_rect_xml}</root>")
    except ET.ParseError:
        return False, ["armorsTab XML parse failed"]

    tabs_contents = root.find(".//rect[@name='tabs']/rect[@name='tabsContents']")
    if tabs_contents is None:
        return False, ["armorsTab missing tabsContents"]

    non_overview_tabs = 0
    for tab in list(tabs_contents):
        if tab.tag != "rect":
            continue
        if tab.attrib.get("controller") != "TabSelectorTab":
            continue
        if tab.attrib.get("tab_key", "") == "agf1MainTabOverview":
            continue

        non_overview_tabs += 1
        tab_name = tab.attrib.get("name", "<unknown>")
        zoom_grid: ET.Element | None = None
        for g in tab.findall("grid"):
            gname = g.attrib.get("name", "")
            if gname.startswith("armor") and gname.endswith("Outfit"):
                zoom_grid = g
                break

        if zoom_grid is None:
            issues.append(f"{tab_name}: missing zoomed outfit grid")
            continue

        rows = zoom_grid.attrib.get("rows", "")
        cols = zoom_grid.attrib.get("cols", "")
        if rows == "6" and cols == "12":
            for q in ("1", "2", "3", "4", "5", "6"):
                hdr = zoom_grid.find(f"./entry[@name='{q}']/label[@name='qualityTier']")
                if hdr is None:
                    issues.append(f"{tab_name}: missing quality header {q}")
        elif rows == "7" and cols == "4":
            q1 = zoom_grid.find("./entry[@name='1q1']/label[@name='qualityTier1']")
            q6 = zoom_grid.find("./entry[@name='1q6']/label[@name='qualityTier6']")
            if q1 is None:
                issues.append(f"{tab_name}: missing quality header q1")
            if q6 is None:
                issues.append(f"{tab_name}: missing quality header q6")
        elif rows == "7" and cols == "8":
            for q in ("1", "2", "3", "4", "5", "6"):
                hdr = zoom_grid.find(f"./entry[@name='1q{q}']/label[@name='qualityTier{q}']")
                if hdr is None:
                    issues.append(f"{tab_name}: missing quality header q{q}")
        else:
            issues.append(
                f"{tab_name}: unexpected grid shape rows={rows or '?'} cols={cols or '?'}"
            )

    if non_overview_tabs == 0:
        issues.append("armorsTab has no non-overview tabs")

    return len(issues) == 0, issues


def _merge_and_template_armors(
    xml_text: str,
    existing_windows_text: str | None,
    fallback_windows_text: str | None,
) -> tuple[str, dict[str, str], str | None, str | None]:
    """Merge armorsTab from first valid source and template compact overview cards."""
    selected_rect: str | None = None
    selected_label: str | None = None
    merge_error: str | None = "no armorsTab source available"

    for label, source_text in (
        ("existing output", existing_windows_text),
        ("seed windows", fallback_windows_text),
    ):
        if not source_text:
            continue
        candidate = _extract_named_rect(source_text, "armorsTab")
        if _is_placeholder_tab(candidate):
            merge_error = f"{label} armorsTab is placeholder or missing"
            continue
        ok, issues = _validate_armors_rect_zoomed_cards(candidate)
        if not ok:
            merge_error = f"{label} armorsTab invalid: {'; '.join(issues[:2])}"
            continue
        selected_rect = candidate
        selected_label = label
        merge_error = None
        break

    if not selected_rect:
        return xml_text, {}, None, merge_error

    # Collect defaults from source armors first, then template may swap to
    # text_key-based rendering.
    armor_setbonus_loc = _collect_armors_setbonus_localization(selected_rect)
    templated = _apply_compact_armor_template_to_armors_rect(selected_rect)

    # Template step should preserve zoomed-card invariants; if not, keep source.
    ok_templated, _issues_templated = _validate_armors_rect_zoomed_cards(templated)
    final_rect = templated if ok_templated else selected_rect

    def _extract_overview_rect(armors_rect_xml: str | None) -> ET.Element | None:
        if not armors_rect_xml:
            return None
        try:
            src_root = ET.fromstring(f"<root>{armors_rect_xml}</root>")
        except ET.ParseError:
            return None
        tabs = src_root.find(".//rect[@name='tabs']/rect[@name='tabsContents']")
        if tabs is None:
            return None
        ov = tabs.find("./rect[@name='allArmors']")
        if ov is None:
            return None
        return ET.fromstring(ET.tostring(ov, encoding="unicode"))

    def _is_known_good_overview_rect(overview_rect: ET.Element | None) -> bool:
        if overview_rect is None:
            return False
        g = overview_rect.find("./grid[@name='armorPrimitiveOutfit']")
        if g is None:
            return False
        if g.attrib.get("rows") != "7" or g.attrib.get("cols") != "4":
            return False
        if g.attrib.get("pos") != "274,-11":
            return False
        if g.attrib.get("cell_width") != "64" or g.attrib.get("cell_height") != "22":
            return False
        l1 = g.find("./entry[@name='1description']/label")
        q1 = g.find("./entry[@name='1q1']/label")
        q6 = g.find("./entry[@name='1q6']/label")
        if l1 is None or q1 is None or q6 is None:
            return False
        if l1.attrib.get("font_size") != "16" or l1.attrib.get("pos") != "24,-11":
            return False
        if q1.attrib.get("font_size") != "16" or q1.attrib.get("pos") != "50,-11":
            return False
        if q6.attrib.get("font_size") != "16" or q6.attrib.get("pos") != "38,-11":
            return False
        return True

    # Restore overview from the best known-good source (matching the pre-removal
    # compact setup), while preserving non-overview zoom tabs from final_rect.
    overview_candidate: ET.Element | None = None
    for armors_src in (
        selected_rect,
        _extract_named_rect(existing_windows_text, "armorsTab") if existing_windows_text else None,
        _extract_named_rect(fallback_windows_text, "armorsTab") if fallback_windows_text else None,
    ):
        ov = _extract_overview_rect(armors_src)
        if _is_known_good_overview_rect(ov):
            overview_candidate = ov
            break

    if overview_candidate is None:
        backup_dir = Path(__file__).resolve().parent / "_AUTOBACKUP"
        for bp in sorted(backup_dir.glob("windows.prewrite.*.xml"), reverse=True):
            try:
                bp_text = bp.read_text(encoding="utf-8")
            except Exception:
                continue
            bp_armors = _extract_named_rect(bp_text, "armorsTab")
            ov = _extract_overview_rect(bp_armors)
            if _is_known_good_overview_rect(ov):
                overview_candidate = ov
                break

    if overview_candidate is not None:
        try:
            merged_root = ET.fromstring(f"<root>{final_rect}</root>")
            merged_tabs = merged_root.find(".//rect[@name='tabs']/rect[@name='tabsContents']")
            if merged_tabs is not None:
                merged_overview = merged_tabs.find("./rect[@name='allArmors']")
                if merged_overview is not None:
                    try:
                        insert_idx = list(merged_tabs).index(merged_overview)
                    except ValueError:
                        insert_idx = 0
                    merged_tabs.remove(merged_overview)
                    merged_tabs.insert(insert_idx, overview_candidate)
                else:
                    merged_tabs.insert(0, overview_candidate)
                final_rect = "".join(ET.tostring(child, encoding="unicode") for child in list(merged_root))
        except ET.ParseError:
            pass

    # Normalize armor type header icons after any overview restoration.
    final_rect = _enforce_armor_header_icons(final_rect)

    return _replace_named_rect(xml_text, "armorsTab", final_rect), armor_setbonus_loc, selected_label, None


def _merge_named_tab_rect(
    xml_text: str,
    tab_name: str,
    existing_windows_text: str | None,
    fallback_windows_text: str | None,
) -> str:
    """Merge a tab rect from existing, otherwise fallback, into generated xml."""
    candidate = _extract_named_rect(existing_windows_text, tab_name) if existing_windows_text else None
    if _is_placeholder_tab(candidate):
        fallback_candidate = _extract_named_rect(fallback_windows_text, tab_name) if fallback_windows_text else None
        if not _is_placeholder_tab(fallback_candidate):
            candidate = fallback_candidate
    if _is_placeholder_tab(candidate):
        return xml_text
    return _replace_named_rect(xml_text, tab_name, candidate)


def _merge_named_tab_rect_from_sources(
    xml_text: str,
    tab_name: str,
    sources: list[tuple[str, str | None]],
    validator=None,
) -> tuple[str, str | None, str | None]:
    """Merge tab_name from the first non-placeholder source.

    A source can be a full windows.xml text or a direct <rect name="..."> block.
    """
    for label, source_text in sources:
        if not source_text:
            continue
        candidate: str | None = None
        stripped = source_text.lstrip()
        if stripped.startswith("<rect"):
            name_match = re.search(r'\bname="([^"]+)"', stripped)
            if name_match and name_match.group(1) == tab_name:
                candidate = stripped
        if candidate is None:
            candidate = _extract_named_rect(source_text, tab_name)
        if _is_placeholder_tab(candidate):
            continue
        if validator is not None:
            ok, reason = validator(candidate)
            if not ok:
                continue
        return _replace_named_rect(xml_text, tab_name, candidate), label, None
    return xml_text, None, "no valid source passed validation"


def _parse_pos_xy(pos: str, default_x: int = 0, default_y: int = 0) -> tuple[int, int]:
    try:
        x_s, y_s = pos.split(",", 1)
        return int(x_s.strip()), int(y_s.strip())
    except Exception:
        return default_x, default_y


def _normalize_shared_all_buttons_and_unlocks_tabs(xml_text: str) -> str:
    """Standardize secondary All buttons and Unlocks non-All centering.

    Rules:
    - Same All button size/position style on Magazines/Books/Unlocks/Armors.
    - All is independent from non-All tab rows.
    - Unlocks non-All tabs keep existing size and are centered with equal
      left/right outer gaps.
    """
    try:
        root = ET.fromstring(xml_text)
    except ET.ParseError:
        return xml_text

    all_button_attrs = {
        "name": "tabButton",
        "depth": "8",
        "pos": "168,33",
        "width": "120",
        "height": "29",
        "font_size": "17",
        "bordercolor": "[black]",
        "defaultcolor": "130, 88, 48",
        "selectedsprite": "menu_empty",
        "selectedcolor": "74, 33, 150",
        "foregroundlayer": "false",
        "caption": "{tab_name_localized}",
        "tooltip_key": "agf1MainTabOverviewTooltip",
    }

    tab_key_map = {
        "craftinglistTab": "agf1MainTabMagazines",
        "booksTab": "agf1MainTabBooks",
        "unlockablesTab": "agf1MainTabUnlocks",
        "armorsTab": "agf1MainTabArmors",
    }

    legacy_tab_key_map = {
        "agfChecklist": "agf1MainTabMagazines",
        "agfBooks": "agf1MainTabBooks",
        "agfUnlocks": "agf1MainTabUnlocks",
        "agfArmors": "agf1MainTabArmors",
        "agfOverview": "agf1MainTabOverview",
        "lblAll": "agf1MainTabOverview",
    }

    # Global remap pass so merged legacy tab blocks cannot keep old key names.
    for tab_rect in root.findall(".//rect[@controller='TabSelectorTab']"):
        old_key = tab_rect.attrib.get("tab_key", "")
        new_key = legacy_tab_key_map.get(old_key) or _migrate_category_key(old_key)
        if new_key:
            tab_rect.set("tab_key", new_key)

    # Normalize category-style text/tooltip keys across merged legacy fragments.
    for el in root.findall(".//*[@text_key]"):
        old_key = el.attrib.get("text_key", "")
        new_key = _migrate_category_key(old_key)
        if new_key != old_key:
            el.set("text_key", new_key)
    for el in root.findall(".//*[@tooltip_key]"):
        old_key = el.attrib.get("tooltip_key", "")
        new_key = _migrate_category_key(old_key)
        if new_key != old_key:
            el.set("tooltip_key", new_key)

    for tab_name in ("craftinglistTab", "booksTab", "unlockablesTab", "armorsTab"):
        tab_rect = root.find(f".//rect[@name='{tab_name}']")
        if tab_rect is None:
            continue

        expected_tab_key = tab_key_map.get(tab_name)
        if expected_tab_key:
            tab_rect.set("tab_key", expected_tab_key)

        tabs_header = tab_rect.find("./rect[@name='tabs']/rect[@name='tabsHeader']")
        if tabs_header is None:
            continue

        tab_grid = tabs_header.find("./grid[@name='tabButtons']")
        if tab_grid is None:
            continue

        all_rect = tabs_header.find("./rect[@name='allTabButton']")
        if all_rect is None:
            all_rect = ET.Element("rect", {"name": "allTabButton", "controller": "TabSelectorButton"})
            insert_at = 0
            try:
                insert_at = list(tabs_header).index(tab_grid)
            except ValueError:
                insert_at = 0
            tabs_header.insert(insert_at, all_rect)
        else:
            all_rect.set("controller", "TabSelectorButton")

        all_btn = all_rect.find("simplebutton")
        if all_btn is None:
            all_btn = ET.Element("simplebutton")
            all_rect.append(all_btn)
        all_btn.attrib.update(all_button_attrs)

        if tab_name == "craftinglistTab":
            # Magazines has 24 tabs including All; move All out and keep 23 in grid.
            old_x, old_y = _parse_pos_xy(tab_grid.attrib.get("pos", "-55,-10"), -55, -10)
            try:
                cols = int(tab_grid.attrib.get("cols", "24"))
            except ValueError:
                cols = 24
            if cols >= 24:
                tab_grid.set("cols", "23")
                # Preserve original non-All visual position after removing the
                # first (All) slot from the grid.
                tab_grid.set("pos", f"{old_x + 60},{old_y}")

        # Retarget the per-page "all" tab key to dedicated overview key so
        # caption localization can be specific without touching global lblAll.
        tabs_contents = tab_rect.find("./rect[@name='tabs']/rect[@name='tabsContents']")
        if tabs_contents is not None:
            for content_tab in list(tabs_contents):
                if content_tab.tag != "rect":
                    continue
                if content_tab.attrib.get("controller") != "TabSelectorTab":
                    continue
                if content_tab.attrib.get("tab_key") in {"lblAll", "agfOverview"}:
                    content_tab.set("tab_key", "agf1MainTabOverview")
                    break

        if tab_name == "unlockablesTab":
            # Unlocks has 7 tabs including All; keep 6 non-All and center row.
            try:
                cols = int(tab_grid.attrib.get("cols", "7"))
            except ValueError:
                cols = 7
            if cols >= 7:
                cols = 6
                tab_grid.set("cols", "6")

            try:
                cell_w = int(tab_grid.attrib.get("cell_width", "166"))
            except ValueError:
                cell_w = 166
            _x, y = _parse_pos_xy(tab_grid.attrib.get("pos", "120,-10"), 120, -10)
            centered_x = max(0, (1390 - cols * cell_w) // 2)
            tab_grid.set("pos", f"{centered_x},{y}")

            # Vanilla unlock source for rocket ammo is craftingExplosives level 45,
            # so both row fills should key off the same progression check cvar.
            for row in tab_rect.findall(".//entry[@name='ammoRowrocket']"):
                for fill in row.findall("./filledsprite[@name='background']"):
                    fill.set("fill", "{cvar(craftingExplosivesCheck45)}")

            for rocket_name in ("ammoRocketFrag", "ammoRocketHE"):
                for entry in tab_rect.findall(f".//entry[@name='{rocket_name}']"):
                    for fill in entry.findall("./filledsprite[@name='background']"):
                        fill.set("fill", "{cvar(craftingExplosivesCheck45)}")

    return ET.tostring(root, encoding="unicode")


def _normalize_global_header_hint(xml_text: str) -> str:
    """Show one shared hint in the top-right header for all main tabs.

    Also removes legacy per-tab hint labels so the message is not duplicated
    or tab-specific.
    """
    try:
        root = ET.fromstring(xml_text)
    except ET.ParseError:
        return xml_text

    tabs_header = root.find(".//window[@name='Schematics']/rect[@name='tabs']/rect[@name='tabsHeader']")
    if tabs_header is not None:
        # Center hint between the end of the rightmost main tab button and
        # the right edge of the header window.
        hint_center_x = 1162
        hint_center_y = 16
        schematics_window = root.find(".//window[@name='Schematics']")
        if schematics_window is not None:
            try:
                header_right_x = int(schematics_window.attrib.get("width", "1390"))
            except ValueError:
                header_right_x = 1390

            main_grid = tabs_header.find("./grid[@name='tabButtons']")
            if main_grid is not None:
                gx, gy = _parse_pos_xy(main_grid.attrib.get("pos", "455,40"), 455, 40)
                try:
                    cols = int(main_grid.attrib.get("cols", "4"))
                except ValueError:
                    cols = 4
                try:
                    cell_w = int(main_grid.attrib.get("cell_width", "120"))
                except ValueError:
                    cell_w = 120

                btn_w = cell_w
                btn = main_grid.find("./rect/simplebutton[@name='tabButton']")
                if btn is not None:
                    try:
                        btn_w = int(btn.attrib.get("width", str(cell_w)))
                    except ValueError:
                        btn_w = cell_w

                if cols > 0:
                    armors_tab_end_x = gx + ((cols - 1) * cell_w) + btn_w
                    hint_center_x = int(round((armors_tab_end_x + header_right_x) / 2.0))

                    # The header uses top-left anchoring for tab buttons.
                    # Convert to center anchor for the hint by subtracting
                    # half the button height in this Y-up coordinate system.
                    btn_h = 40
                    by = 0
                    if btn is not None:
                        try:
                            btn_h = int(btn.attrib.get("height", "40"))
                        except ValueError:
                            btn_h = 40
                        bx_s, by_s = _parse_pos_xy(btn.attrib.get("pos", "0,0"), 0, 0)
                        by = by_s
                    hint_center_y = gy + by - int(round(btn_h / 2.0))

        # Rebuild hint elements each pass to keep layout deterministic.
        for child in list(tabs_header):
            if child.tag not in {"sprite", "label"}:
                continue
            if child.attrib.get("name") in {
                "headerHintBg",
                "headerHintBorder",
                "headerHintShadow",
                "headerHint",
            }:
                tabs_header.remove(child)

        hint_label = ET.Element("label", {
            "name": "headerHint",
            "depth": "5",
            "justify": "center",
            "pivot": "center",
            "pos": f"{hint_center_x},{hint_center_y}",
            "width": "360",
            "height": "32",
            "foregroundlayer": "true",
            "color": "221, 205, 250",
            "text_key": "agf1MainHeaderHint",
            "font_size": "22",
            "effect": "Outline8",
            "effect_color": "0,0,0,255",
            "effect_distance": "1,1",
        })
        tabs_header.append(hint_label)

    # Remove old per-main-tab tip labels now that the hint is global.
    legacy_tip_keys = {"checklistZoomArmor", "checklistZoomMagazine", "checklistHovering"}
    for tab_name in ("craftinglistTab", "booksTab", "unlockablesTab", "armorsTab"):
        tab_rect = root.find(f".//rect[@name='{tab_name}']")
        if tab_rect is None:
            continue
        for child in list(tab_rect):
            if child.tag != "label":
                continue
            text_key = child.attrib.get("text_key", "")
            if child.attrib.get("name") == "headernote" or text_key in legacy_tip_keys:
                tab_rect.remove(child)

    return ET.tostring(root, encoding="unicode")


def _extract_loc_key(line: str) -> str | None:
    s = line.lstrip("\ufeff").strip()
    if not s or s.startswith("#"):
        return None
    if "," not in s:
        return None
    key = s.split(",", 1)[0].strip()
    # Normalize legacy quoted/unquoted key variants to a single key identity.
    if len(key) >= 2 and key[0] == '"' and key[-1] == '"':
        key = key[1:-1].replace('""', '"').strip()
    return key


def _parse_loc_map(loc_text: str) -> tuple[str, dict[str, str], list[str]]:
    lines = loc_text.splitlines()
    header = lines[0].lstrip("\ufeff") if lines else "Key,File,Type,UsedInMainMenu,NoTranslate,english"
    key_to_line: dict[str, str] = {}
    order: list[str] = []
    for ln in lines[1:]:
        key = _extract_loc_key(ln)
        if not key:
            continue
        if key not in key_to_line:
            order.append(key)
        key_to_line[key] = ln
    return header, key_to_line, order


def _replace_loc_line_key(line: str, new_key: str) -> str:
    parts = line.split(",", 1)
    if len(parts) == 2:
        return f"{new_key},{parts[1]}"
    return f"{new_key},,,,,\"\""


def _merge_localization_text(
    generated_min_text: str,
    existing_text: str | None,
    fallback_text: str | None,
    active_armor_names: set[str] | None = None,
) -> str:
    """Merge localization while preserving rich historical entries.

    Preference order:
    1) existing Localization.txt if it already appears rich
    2) fallback Localization.txt from release source
    3) generated minimal localization
    """
    gen_header, gen_map, gen_order = _parse_loc_map(generated_min_text)

    # Normalize generated key names before merge so deprecated variants never
    # get reintroduced when generated defaults are applied.
    normalized_gen_map: dict[str, str] = {}
    normalized_gen_order: list[str] = []
    for old_key in gen_order:
        if old_key not in gen_map:
            continue
        new_key = _migrate_category_key(old_key)
        if new_key in normalized_gen_map:
            continue
        normalized_gen_map[new_key] = _replace_loc_line_key(gen_map[old_key], new_key)
        normalized_gen_order.append(new_key)
    gen_map = normalized_gen_map
    gen_order = normalized_gen_order
    deprecated_localization_keys = {
        "checklistHovering",
        "checklistZoomArmor",
        "checklistZoomMagazine",
        "agfChecklist",
        "agfBooks",
        "agfUnlocks",
        "agfArmors",
        "agfOverview",
        "agfAllOverviewTooltip",
        "agfHeaderHint",
        "lblAll",
        "agfNerdOutfittDescription",
        "agfPrimitiveDescription",
        "agfStatMobility",
        "agfStatNoiseIncrease",
        "agfStatStaminaChangeOT",
        "agfArmorSamuraiOutfit",
        "agfArmorSantaHatHelmetDescription",
        "agfFitnessBarteringDescription",
        "poweredDoors",
        "agfPoweredDoors",
        "agfArmorAssassinSetBonus",
        "agfArmorAthleticSetBonus",
        "agfArmorBikerSetBonus",
        "agfArmorCommandoSetBonus",
        "agfArmorEnforcerSetBonus",
        "agfArmorFarmerSetBonus",
        "agfArmorLumberjackSetBonus",
        "agfArmorMinerSetBonus",
        "agfArmorNerdSetBonus",
        "agfArmorNomadSetBonus",
        "agfArmorPreacherSetBonus",
        "agfArmorPrimitiveSetBonus",
        "agfArmorRaiderSetBonus",
        "agfArmorRangerSetBonus",
        "agfArmorRogueSetBonus",
        "agfArmorScavengerSetBonus",
    }

    seed_text: str | None = None
    if existing_text:
        _h, ex_map, _o = _parse_loc_map(existing_text)
        if (
            len(ex_map) >= 20
            or "AGFarmorBikerOutfit" in ex_map
            or "agfHeaderHint" in ex_map
            or "agfMainOverviewHeaderHint" in ex_map
            or "agf1MainHeaderHint" in ex_map
        ):
            seed_text = existing_text
    if seed_text is None and fallback_text:
        seed_text = fallback_text
    if seed_text is None:
        seed_text = generated_min_text

    seed_header, seed_map, seed_order = _parse_loc_map(seed_text)

    # Promote legacy unlock/armor category keys to categorized key names while
    # preserving any existing translated row content.
    for old_key in list(seed_map.keys()):
        new_key = _migrate_category_key(old_key)
        if new_key == old_key:
            continue
        old_line = seed_map.pop(old_key)
        seed_order = [new_key if k == old_key else k for k in seed_order]
        if new_key not in seed_map:
            seed_map[new_key] = _replace_loc_line_key(old_line, new_key)
            if new_key not in seed_order:
                seed_order.append(new_key)

    # Always apply generator-authored labels for explicitly managed unlock headers.
    force_generated_keys = {
        "xuiSchematics",
        "agf0PurpleBookButton",
        "agf0PurpleBookButtonTooltip",
        "agf1MainHeaderHint",
        "agf1MainTabArmors",
        "agf1MainTabBooks",
        "agf1MainTabMagazines",
        "agf1MainTabOverview",
        "agf1MainTabOverviewTooltip",
        "agf1MainTabUnlocks",
        "agf4UnlocksCategoryAmmo",
        "agf4UnlocksCategoryAmmoRow44",
        "agf4UnlocksCategoryAmmoRow762",
        "agf4UnlocksCategoryAmmoRow9mm",
        "agf4UnlocksCategoryAmmoRowArrows",
        "agf4UnlocksCategoryAmmoRowBolts",
        "agf4UnlocksCategoryAmmoRowJunkTurret",
        "agf4UnlocksCategoryAmmoRowRockets",
        "agf4UnlocksCategoryAmmoRowShotgun",
        "agf4UnlocksCategoryArmors",
        "agf4UnlocksCategoryArmorsBoots",
        "agf4UnlocksCategoryArmorsFittings",
        "agf4UnlocksCategoryArmorsGeneral",
        "agf4UnlocksCategoryArmorsHelmet",
        "agf4UnlocksCategoryArmorsMuffledConnectors",
        "agf4UnlocksCategoryArmorsPlating",
        "agf4UnlocksCategoryArmorsPockets",
        "agf4UnlocksCategoryDrone",
        "agf4UnlocksCategoryMisc",
        "agf4UnlocksCategoryToolsWeapons",
        "agf4UnlocksCategoryToolsWeaponsBarrels",
        "agf4UnlocksCategoryToolsWeaponsBlockDamage",
        "agf4UnlocksCategoryToolsWeaponsClub",
        "agf4UnlocksCategoryToolsWeaponsMotorTool",
        "agf4UnlocksCategoryToolsWeaponsOtherGun",
        "agf4UnlocksCategoryToolsWeaponsOtherMelee",
        "agf4UnlocksCategoryToolsWeaponsScopes",
        "agf4UnlocksCategoryToolsWeaponsShotgun",
        "agf4UnlocksCategoryToolsWeaponsSpecial",
        "agf4UnlocksCategoryVehicles",
        "agf5ArmorsRatingHeavy",
        "agf5ArmorsRatingLight",
        "agf5ArmorsRatingMedium",
        "agf5ArmorSetBonusTooltip",
    }

    # Ensure generated baseline keys exist without clobbering user edits.
    for k in gen_order:
        if k not in seed_map:
            seed_map[k] = gen_map[k]
            if k not in seed_order:
                seed_order.append(k)

    for k in force_generated_keys:
        if k in gen_map:
            seed_map[k] = gen_map[k]
            if k not in seed_order:
                seed_order.append(k)

    # Generated armor-piece description rows are authoritative and must stay
    # stripped of any full-set bonus paragraph from vanilla source text.
    for k, v in gen_map.items():
        if _is_generated_armor_piece_desc_key(k):
            seed_map[k] = v
            if k not in seed_order:
                seed_order.append(k)

    # Seed/repair armor set-bonus keys from generated defaults without
    # clobbering existing user-edited values.
    for k, v in gen_map.items():
        if _is_setbonus_localization_key(k) and k.endswith("SetBonusDesc"):
            seed_map[k] = v
            if k not in seed_order:
                seed_order.append(k)
            continue
        if _is_setbonus_localization_key(k) and re.search(r"SetBonus[12]$", k):
            seed_map[k] = v
            if k not in seed_order:
                seed_order.append(k)
            continue
        if _is_setbonus_localization_key(k):
            current = (seed_map.get(k, "") or "").strip().lower()
            if (k not in seed_map) or (current in ("", "none")):
                seed_map[k] = v
                if k not in seed_order:
                    seed_order.append(k)

    # Remove deprecated tip keys that are no longer used by current UI.
    for k in deprecated_localization_keys:
        seed_map.pop(k, None)
    seed_order = [k for k in seed_order if k not in deprecated_localization_keys]

    # Remove armor localization keys that are not represented by active sets
    # in armorsTab, so cosmetics/legacy rows do not reappear via merge.
    for k in list(seed_map.keys()):
        if _is_nonactive_armor_localization_key(k, active_armor_names):
            seed_map.pop(k, None)
    seed_order = [k for k in seed_order if not _is_nonactive_armor_localization_key(k, active_armor_names)]

    header = seed_header or gen_header
    ordered_keys = sorted(seed_map.keys(), key=lambda k: k.lower())
    out_lines = [header] + [seed_map[k] for k in ordered_keys if k in seed_map]
    return "\n".join(out_lines) + "\n"


def _normalize_localization_csv_style(loc_text: str) -> str:
    """Normalize CSV output style for stable, scannable localization diffs.

    Rules:
    - Header is emitted as plain comma-separated fields.
    - Language columns are always quoted.
    - Non-language columns are only quoted when CSV escaping is required.
    """
    if not (loc_text or "").strip():
        return loc_text

    try:
        reader = csv.reader(io.StringIO(loc_text))
        rows = list(reader)
    except csv.Error:
        return loc_text

    if not rows:
        return loc_text

    header = rows[0][:]
    if header:
        header[0] = (header[0] or "").lstrip("\ufeff")
    ncols = len(header)
    if ncols <= 0:
        return loc_text

    language_indices = {
        idx for idx, col in enumerate(header)
        if (col or "").strip().lower() in LOCALIZATION_LANGUAGE_HEADERS
    }

    out_lines = [",".join(header)]
    for row in rows[1:]:
        if len(row) < ncols:
            row = row + ([""] * (ncols - len(row)))
        elif len(row) > ncols:
            row = row[:ncols]
        out_lines.append(_csv_row_to_line_language_quoted(row, language_indices))

    return "\n".join(out_lines) + "\n"


def _validate_required_tabs(xml_text: str) -> list[str]:
    """Return blocking issues if required tabs are missing/regressed."""
    issues: list[str] = []
    books_rect = _extract_named_rect(xml_text, "booksTab")
    unlocks_rect = _extract_named_rect(xml_text, "unlockablesTab")

    if _is_placeholder_tab(books_rect):
        issues.append("booksTab is placeholder or missing")
    if _is_placeholder_tab(unlocks_rect):
        issues.append("unlockablesTab is placeholder or missing")

    if books_rect and "zoomedBooks" not in books_rect:
        issues.append("booksTab missing zoomedBooks content")
    if unlocks_rect and "unlockAll" not in unlocks_rect:
        issues.append("unlockablesTab missing unlockAll page")
    if unlocks_rect and "agf4UnlocksCategoryAmmo" not in unlocks_rect:
        issues.append("unlockablesTab missing agf4UnlocksCategoryAmmo page")
    return issues


def _normalize_unlock_review_category(value: str, fallback: str = "") -> str:
    raw = (value or "").strip()
    if not raw:
        return fallback
    for option in UNLOCK_REVIEW_TOP_CATEGORY_ORDER:
        if raw.lower() == option.lower():
            return option
    aliases = {
        "armor": "Armors",
        "armors": "Armors",
        "tool": "Tools & Weapons",
        "tools": "Tools & Weapons",
        "weapon": "Tools & Weapons",
        "weapons": "Tools & Weapons",
        "tools and weapons": "Tools & Weapons",
        "tools & weapons": "Tools & Weapons",
        "vehicle": "Vehicles",
        "vehicles": "Vehicles",
        "drone": "Drones",
        "drones": "Drones",
        "miscellaneous": "Misc",
        "misc": "Misc",
        "ammo": "Ammo",
    }
    return aliases.get(raw.lower(), fallback)


def _normalize_unlock_review_subcategory(category: str, value: str, fallback: str = "") -> str:
    options = UNLOCK_REVIEW_SUBCATEGORY_OPTIONS.get(category, [])
    if not options:
        return ""
    raw = (value or "").strip()
    if not raw:
        return fallback if fallback in options else ""
    for option in options:
        if raw.lower() == option.lower():
            return option
    aliases = {
        "44 ammo": ".44 Ammo",
        "44": ".44 Ammo",
    }
    mapped = aliases.get(raw.lower(), "")
    if mapped in options:
        return mapped
    return fallback if fallback in options else ""


def _coerce_bool(value: typing.Any, default: bool | None = None) -> bool | None:
    if isinstance(value, bool):
        return value
    if isinstance(value, str):
        v = value.strip().lower()
        if v in {"1", "true", "t", "y", "yes", "on"}:
            return True
        if v in {"0", "false", "f", "n", "no", "off"}:
            return False
    return default


def _coerce_positive_int(value: typing.Any, default: int = 0) -> int:
    try:
        out = int(str(value).strip())
    except Exception:
        return default
    return out if out > 0 else default


def _candidate_fingerprint(candidate: UnlockReviewCandidate) -> str:
    sub = candidate.detected_subcategory or ""
    return (
        f"{candidate.source_tab_key}|{candidate.detected_category}|"
        f"{sub}|{candidate.detected_order}"
    )


def _candidate_name_key(name: str) -> str:
    return (name or "").strip().lower()


def _load_unlock_review_ledger(path: Path) -> dict[str, dict[str, typing.Any]]:
    items: dict[str, dict[str, typing.Any]] = {}
    if not path.exists():
        return items
    try:
        payload = json.loads(path.read_text(encoding="utf-8"))
    except Exception:
        return items

    raw_items: typing.Any = payload
    if isinstance(payload, dict) and isinstance(payload.get("items"), dict):
        raw_items = payload.get("items")

    if isinstance(raw_items, dict):
        iterable = raw_items.items()
    elif isinstance(raw_items, list):
        iterable = [((row.get("name") or ""), row) for row in raw_items if isinstance(row, dict)]
    else:
        iterable = []

    for key, value in iterable:
        if not isinstance(value, dict):
            continue
        name = (value.get("name") or str(key or "")).strip()
        if not name:
            continue
        record = dict(value)
        record["name"] = name
        items[name] = record
    return items


def _save_unlock_review_ledger(path: Path, items: dict[str, dict[str, typing.Any]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    payload: dict[str, typing.Any] = {
        "version": 1,
        "updated_utc": datetime.datetime.utcnow().replace(microsecond=0).isoformat() + "Z",
        "items": {},
    }
    for name in sorted(items.keys(), key=lambda n: n.lower()):
        record = dict(items[name])
        record["name"] = name
        payload["items"][name] = record
    path.write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")


def _extract_accounted_unlock_review_candidates(
    xml_text: str,
    recipe_names: set[str],
    recipes_loaded: bool,
) -> list[UnlockReviewCandidate]:
    def _parse_root(raw_text: str) -> ET.Element | None:
        try:
            return ET.fromstring(raw_text)
        except ET.ParseError:
            try:
                return ET.fromstring(f"<root>{raw_text}</root>")
            except ET.ParseError:
                return None

    root = _parse_root(xml_text)
    if root is None:
        return []

    unlock_rect: ET.Element | None = None
    if root.tag == "rect" and root.attrib.get("name") == "unlockablesTab":
        unlock_rect = root
    if unlock_rect is None:
        unlock_rect = root.find(".//rect[@name='unlockablesTab']")
    if unlock_rect is None:
        return []

    tabs_contents = unlock_rect.find("./rect[@name='tabs']/rect[@name='tabsContents']")
    if tabs_contents is None:
        tabs_contents = unlock_rect.find("./rect[@name='tabsContents']")
    if tabs_contents is None:
        tabs_contents = unlock_rect

    unlock_all = tabs_contents.find("./rect[@name='unlockAll']")
    all_names: list[str] = []
    seen_all: set[str] = set()
    if unlock_all is not None:
        for entry in unlock_all.findall(".//entry[@name]"):
            nm = (entry.attrib.get("name") or "").strip()
            if not nm:
                continue
            nm_key = _candidate_name_key(nm)
            if nm_key in seen_all:
                continue
            if _is_uncraftable_item_modifier(nm, recipe_names, recipes_loaded):
                continue
            seen_all.add(nm_key)
            all_names.append(nm)

    placements: dict[str, tuple[str, str, int, str]] = {}
    placement_names: dict[str, str] = {}
    placement_order = 0

    def _remember(name: str, category: str, subcategory: str, tab_key: str) -> None:
        nonlocal placement_order
        nm = (name or "").strip()
        if not nm:
            return
        nm_key = _candidate_name_key(nm)
        if nm_key in placements:
            return
        if _is_uncraftable_item_modifier(nm, recipe_names, recipes_loaded):
            return
        placement_order += 1
        placements[nm_key] = (category, subcategory, placement_order, tab_key)
        placement_names[nm_key] = nm

    for page in tabs_contents.findall("./rect"):
        page_name = (page.attrib.get("name") or "").strip()
        if page_name == "unlockAll":
            continue
        tab_key = (page.attrib.get("tab_key") or "").strip()
        top_category = UNLOCK_REVIEW_TOP_CATEGORY_BY_TAB_KEY.get(tab_key, "")
        if not top_category:
            continue

        current_subcategory = ""
        for child in list(page):
            if child.tag == "label":
                text_key = (child.attrib.get("text_key") or "").strip()
                mapped_subcategory = UNLOCK_REVIEW_SUBCATEGORY_BY_TEXT_KEY.get(text_key, "")
                if mapped_subcategory:
                    current_subcategory = mapped_subcategory
                continue

            if child.tag == "grid":
                for entry in child.findall("./entry[@name]"):
                    _remember(
                        (entry.attrib.get("name") or "").strip(),
                        top_category,
                        current_subcategory,
                        tab_key,
                    )
                continue

            if child.tag == "entry":
                row_subcategory = current_subcategory
                row_label = child.find("./label[@text_key]")
                if row_label is not None:
                    row_text_key = (row_label.attrib.get("text_key") or "").strip()
                    mapped_subcategory = UNLOCK_REVIEW_SUBCATEGORY_BY_TEXT_KEY.get(row_text_key, "")
                    if mapped_subcategory:
                        row_subcategory = mapped_subcategory

                seen_row_icons: set[str] = set()
                for icon in child.findall(".//sprite[@name='itemIcon']"):
                    tooltip_key = (icon.attrib.get("tooltip_key") or "").strip()
                    if not tooltip_key:
                        continue
                    if tooltip_key.lower().startswith("ammobundle"):
                        continue
                    tooltip_key_norm = _candidate_name_key(tooltip_key)
                    if tooltip_key_norm in seen_row_icons:
                        continue
                    seen_row_icons.add(tooltip_key_norm)
                    _remember(tooltip_key, top_category, row_subcategory, tab_key)

    ordered_names: list[str]
    if all_names:
        ordered_names = list(all_names)
        seen_ordered = {_candidate_name_key(name) for name in ordered_names}
        for name_key, _placement in sorted(placements.items(), key=lambda kv: kv[1][2]):
            if name_key not in seen_ordered:
                ordered_names.append(placement_names.get(name_key, name_key))
    else:
        ordered_names = [placement_names.get(name_key, name_key) for name_key, _placement in sorted(placements.items(), key=lambda kv: kv[1][2])]

    candidates: list[UnlockReviewCandidate] = []
    for idx, raw_name in enumerate(ordered_names, start=1):
        nm = raw_name.strip()
        nm_key = _candidate_name_key(nm)
        if nm_key in placements:
            detected_category, detected_subcategory, detected_order, source_tab_key = placements[nm_key]
        else:
            detected_category = _infer_unlock_review_category_from_name(nm)
            detected_subcategory = _infer_unlock_review_subcategory_from_name(detected_category, nm)
            detected_order = idx
            source_tab_key = _unlock_review_tab_key_for_category(detected_category)
        candidates.append(
            UnlockReviewCandidate(
                name=nm,
                detected_category=detected_category,
                detected_subcategory=detected_subcategory,
                detected_order=detected_order,
                source_tab_key=source_tab_key,
                discovered_from_game_scan=False,
                discovered_sources=["accounted_unlock_tab"],
            )
        )

    candidates.sort(key=lambda c: (c.detected_order, c.name.lower()))
    return candidates


def _discover_game_unlock_review_candidates(
    items_path: Path,
    progression_path: Path,
    recipes_path: Path,
    recipe_names: set[str],
    recipes_loaded: bool,
) -> list[UnlockReviewCandidate]:
    discovered: dict[str, UnlockReviewCandidate] = {}
    next_order = 0
    allowed_recipe_names: set[str] = set()
    recipe_name_by_key: dict[str, str] = {}
    recipe_order: list[str] = []

    def _is_recipe_magazine_or_progression_tag(tag: str) -> bool:
        t = (tag or "").strip().lower()
        if not t:
            return False
        if t.startswith("perk"):
            return True
        if t.startswith("crafting"):
            return True
        if t.endswith("skillmagazine"):
            return True
        return "magazine" in t

    def _is_schematic_or_book_source_item(name: str) -> bool:
        nm = _candidate_name_key(name)
        if not nm:
            return False
        if "magazine" in nm:
            return False
        if "schematic" in nm:
            return True
        if nm.startswith("book"):
            return True
        return "book" in nm

    def _is_excluded_ammo_recipe_name(name: str) -> bool:
        nm = _candidate_name_key(name)
        if "gascan" in nm:
            return True
        if nm.startswith("thrown"):
            return True
        if "dart" in nm:
            return True
        return False

    def _is_rocket_ammo_recipe_name(name: str) -> bool:
        nm = _candidate_name_key(name)
        return nm.startswith("ammorocket")

    def _remember(name: str, source: str) -> None:
        nonlocal next_order
        nm = (name or "").strip()
        if not nm:
            return
        nm_key = _candidate_name_key(nm)
        if nm_key not in allowed_recipe_names:
            return
        if not _is_unlock_review_game_candidate(nm):
            return
        if _is_uncraftable_item_modifier(nm, recipe_names, recipes_loaded):
            return

        existing = discovered.get(nm_key)
        if existing is not None:
            if source not in existing.discovered_sources:
                existing.discovered_sources.append(source)
            return

        next_order += 1
        detected_category = _infer_unlock_review_category_from_name(nm)
        detected_subcategory = _infer_unlock_review_subcategory_from_name(detected_category, nm)
        discovered[nm_key] = UnlockReviewCandidate(
            name=nm,
            detected_category=detected_category,
            detected_subcategory=detected_subcategory,
            detected_order=next_order,
            source_tab_key=_unlock_review_tab_key_for_category(detected_category),
            discovered_from_game_scan=True,
            discovered_sources=[source],
        )

    recipe_meta: dict[str, dict[str, bool]] = {}

    # Recipes are source-of-truth for craftability and default/learnable state.
    try:
        recipes_root = ET.parse(recipes_path).getroot()
    except (FileNotFoundError, ET.ParseError):
        recipes_root = None
    if recipes_root is not None:
        for rec in recipes_root.iter("recipe"):
            nm = (rec.get("name") or "").strip()
            if not nm:
                continue
            nm_key = _candidate_name_key(nm)
            if nm_key not in recipe_name_by_key:
                recipe_name_by_key[nm_key] = nm
                recipe_order.append(nm_key)

            tags = {t.strip().lower() for t in _split_csv(rec.get("tags", ""))}
            meta = recipe_meta.setdefault(
                nm_key,
                {
                    "is_ammo": False,
                    "has_default": False,
                    "has_learnable": False,
                    "has_mag_or_progression_tags": False,
                },
            )
            if _classify_unlock_category(nm) == "Ammo":
                meta["is_ammo"] = True
            if "learnable" in tags:
                meta["has_learnable"] = True
            else:
                meta["has_default"] = True
            if any(_is_recipe_magazine_or_progression_tag(tag) for tag in tags):
                meta["has_mag_or_progression_tags"] = True

    schematic_book_unlock_names: set[str] = set()

    # Items provide schematic/book -> Unlocks mapping.
    try:
        items_root = ET.parse(items_path).getroot()
    except (FileNotFoundError, ET.ParseError):
        items_root = None
    if items_root is not None:
        for item in items_root.iter("item"):
            item_name = (item.get("name") or "").strip()
            if not _is_schematic_or_book_source_item(item_name):
                continue
            for prop in item.findall("property"):
                if (prop.get("name") or "") != "Unlocks":
                    continue
                for unlocked in _split_csv(prop.get("value", "")):
                    unlocked_key = _candidate_name_key(unlocked)
                    if unlocked_key:
                        schematic_book_unlock_names.add(unlocked_key)

    # Pass/fail contract:
    # - Ammo: include default ammo recipes, all rocket ammo recipes, and
    #   non-magazine/perk/crafting learned ammo, except gas/thrown/darts.
    # - Non-ammo: include only schematic/book unlock recipes that are not
    #   already default craftable and not magazine/perk/crafting gated.
    for nm_key, meta in recipe_meta.items():
        nm = recipe_name_by_key.get(nm_key, nm_key)
        if meta["is_ammo"]:
            if _is_excluded_ammo_recipe_name(nm):
                continue
            if meta["has_default"]:
                allowed_recipe_names.add(nm_key)
                continue
            if _is_rocket_ammo_recipe_name(nm):
                allowed_recipe_names.add(nm_key)
                continue
            if meta["has_learnable"] and not meta["has_mag_or_progression_tags"]:
                allowed_recipe_names.add(nm_key)
            continue

        if nm_key not in schematic_book_unlock_names:
            continue
        if meta["has_default"]:
            continue
        if meta["has_mag_or_progression_tags"]:
            continue
        allowed_recipe_names.add(nm_key)

    # Seed allowed recipes in recipe file order.
    for nm_key in recipe_order:
        if nm_key in allowed_recipe_names:
            _remember(recipe_name_by_key.get(nm_key, nm_key), "recipes.allowed")

    # Add source metadata from schematic/book Unlocks entries.
    if items_root is not None:
        for item in items_root.iter("item"):
            item_name = (item.get("name") or "").strip()
            if not _is_schematic_or_book_source_item(item_name):
                continue
            for prop in item.findall("property"):
                if (prop.get("name") or "") != "Unlocks":
                    continue
                for unlocked in _split_csv(prop.get("value", "")):
                    _remember(unlocked, "items.unlocks.schematic_or_book")

    # Progression unlock entries are supplemental discovery.
    try:
        progression_root = ET.parse(progression_path).getroot()
    except (FileNotFoundError, ET.ParseError):
        progression_root = None
    if progression_root is not None:
        for unlock_entry in progression_root.iter("unlock_entry"):
            for unlocked in _split_csv(unlock_entry.get("item", "")):
                _remember(unlocked, "progression.unlock_entry.item")
            for unlocked in _split_csv(unlock_entry.get("recipes", "")):
                _remember(unlocked, "progression.unlock_entry.recipes")

    out = list(discovered.values())
    out.sort(key=lambda c: (c.detected_order, c.name.lower()))
    return out


def _has_complete_unlock_review_decision(record: dict[str, typing.Any]) -> bool:
    include = _coerce_bool(record.get("include"), default=None)
    if include is None:
        return False
    if include is False:
        return True

    category = _normalize_unlock_review_category(str(record.get("category") or ""), fallback="")
    if not category:
        return False

    sub_options = UNLOCK_REVIEW_SUBCATEGORY_OPTIONS.get(category, [])
    if sub_options:
        subcategory = _normalize_unlock_review_subcategory(
            category,
            str(record.get("subcategory") or ""),
            fallback="",
        )
        if not subcategory:
            return False

    order = _coerce_positive_int(record.get("order"), default=0)
    return order > 0


def _compute_unlock_review_next_order(
    ledger_items: dict[str, dict[str, typing.Any]],
    category: str,
    subcategory: str,
) -> int:
    normalized_category = _normalize_unlock_review_category(category, fallback="")
    if not normalized_category:
        return 1

    options = UNLOCK_REVIEW_SUBCATEGORY_OPTIONS.get(normalized_category, [])
    normalized_subcategory = _normalize_unlock_review_subcategory(
        normalized_category,
        subcategory,
        fallback="",
    )
    max_order = 0
    for record in ledger_items.values():
        include = _coerce_bool(record.get("include"), default=None)
        if include is not True:
            continue
        record_category = _normalize_unlock_review_category(str(record.get("category") or ""), fallback="")
        if record_category != normalized_category:
            continue
        if options:
            record_subcategory = _normalize_unlock_review_subcategory(
                normalized_category,
                str(record.get("subcategory") or ""),
                fallback="",
            )
            if record_subcategory != normalized_subcategory:
                continue
        order = _coerce_positive_int(record.get("order"), default=0)
        if order > max_order:
            max_order = order
    return max(1, max_order + 1)


def _compute_unlock_review_alphabetical_order(
    ledger_items: dict[str, dict[str, typing.Any]],
    category: str,
    subcategory: str,
    candidate_name: str,
) -> int:
    normalized_category = _normalize_unlock_review_category(category, fallback="")
    if not normalized_category:
        return 1

    options = UNLOCK_REVIEW_SUBCATEGORY_OPTIONS.get(normalized_category, [])
    normalized_subcategory = _normalize_unlock_review_subcategory(
        normalized_category,
        subcategory,
        fallback="",
    )

    names: set[str] = set()
    for record in ledger_items.values():
        include = _coerce_bool(record.get("include"), default=None)
        if include is not True:
            continue
        record_name = (record.get("name") or "").strip()
        if not record_name:
            continue
        record_category = _normalize_unlock_review_category(str(record.get("category") or ""), fallback="")
        if record_category != normalized_category:
            continue
        if options:
            record_subcategory = _normalize_unlock_review_subcategory(
                normalized_category,
                str(record.get("subcategory") or ""),
                fallback="",
            )
            if record_subcategory != normalized_subcategory:
                continue
        names.add(record_name)

    candidate_clean = (candidate_name or "").strip()
    if candidate_clean:
        names.add(candidate_clean)
    if not names:
        return 1

    ordered_names = sorted(names, key=lambda value: value.lower())
    try:
        return ordered_names.index(candidate_clean) + 1
    except ValueError:
        return max(1, len(ordered_names))


def _prompt_yes_no(prompt: str, default: bool = True) -> bool:
    suffix = "[Y/n]" if default else "[y/N]"
    while True:
        raw = input(f"{prompt} {suffix} ").strip().lower()
        if not raw:
            return default
        if raw in {"y", "yes"}:
            return True
        if raw in {"n", "no"}:
            return False
        print("Please enter y or n.")


def _prompt_select_option(prompt: str, options: list[str], default: str = "") -> str:
    if not options:
        return ""
    normalized_default = default if default in options else ""
    while True:
        print(f"{prompt}:")
        for idx, option in enumerate(options, start=1):
            marker = " (default)" if option == normalized_default and normalized_default else ""
            print(f"  {idx}. {option}{marker}")
        raw = input("> ").strip()
        if not raw:
            if normalized_default:
                return normalized_default
            print("Please choose an option.")
            continue
        if raw.isdigit():
            choice = int(raw)
            if 1 <= choice <= len(options):
                return options[choice - 1]
        for option in options:
            if raw.lower() == option.lower():
                return option
        print("Please choose a valid option number or exact option text.")


def _prompt_positive_int(prompt: str, default: int, alphabetical: int | None = None) -> int:
    fallback = max(1, default)
    alpha_fallback = max(1, alphabetical) if alphabetical is not None else None
    while True:
        hint = ""
        if alpha_fallback is not None:
            hint = f" (or default/alphabetical={alpha_fallback})"
        else:
            hint = " (or default)"
        raw = input(f"{prompt} [{fallback}]{hint}: ").strip()
        if not raw:
            return fallback
        lowered = raw.lower()
        if lowered in {"default", "d"}:
            return fallback
        if alpha_fallback is not None and lowered in {"alphabetical", "alpha", "a", "abc"}:
            return alpha_fallback
        try:
            value = int(raw)
        except ValueError:
            value = 0
        if value > 0:
            return value
        if alpha_fallback is not None:
            print("Please enter a positive integer, 'default', or 'alphabetical'.")
        else:
            print("Please enter a positive integer or 'default'.")


def _seed_accounted_unlock_review_items(
    accounted_candidates: list[UnlockReviewCandidate],
    ledger_items: dict[str, dict[str, typing.Any]],
    ledger_path: Path,
) -> int:
    seeded = 0
    changed = False
    for candidate in accounted_candidates:
        existing = ledger_items.get(candidate.name)
        if existing and _has_complete_unlock_review_decision(existing):
            continue

        category = _normalize_unlock_review_category(candidate.detected_category, fallback="Misc")
        sub_options = UNLOCK_REVIEW_SUBCATEGORY_OPTIONS.get(category, [])
        if sub_options:
            subcategory = _normalize_unlock_review_subcategory(
                category,
                candidate.detected_subcategory,
                fallback="",
            )
            if not subcategory:
                subcategory = sub_options[0]
        else:
            subcategory = ""
        order = max(1, candidate.detected_order)

        ledger_items[candidate.name] = {
            "name": candidate.name,
            "include": True,
            "category": category,
            "subcategory": subcategory,
            "order": order,
            "detected_category": candidate.detected_category,
            "detected_subcategory": candidate.detected_subcategory,
            "detected_order": candidate.detected_order,
            "source_tab_key": candidate.source_tab_key,
            "fingerprint": _candidate_fingerprint(candidate),
            "discovered_from_game_scan": False,
            "discovered_sources": ["accounted_unlock_tab"],
            "auto_seeded_from_existing_tab": True,
            "updated_utc": datetime.datetime.utcnow().replace(microsecond=0).isoformat() + "Z",
        }
        seeded += 1
        changed = True

    if changed:
        _save_unlock_review_ledger(ledger_path, ledger_items)
    return seeded


def _run_unlock_review_session(
    candidates: list[UnlockReviewCandidate],
    ledger_items: dict[str, dict[str, typing.Any]],
    ledger_path: Path,
    max_items: int,
) -> tuple[int, int]:
    pending: list[UnlockReviewCandidate] = []
    metadata_changed = False

    def _build_record(
        candidate: UnlockReviewCandidate,
        *,
        include: bool,
        category: str,
        subcategory: str,
        order: int,
    ) -> dict[str, typing.Any]:
        return {
            "name": candidate.name,
            "include": include,
            "category": category,
            "subcategory": subcategory,
            "order": order,
            "detected_category": candidate.detected_category,
            "detected_subcategory": candidate.detected_subcategory,
            "detected_order": candidate.detected_order,
            "source_tab_key": candidate.source_tab_key,
            "fingerprint": _candidate_fingerprint(candidate),
            "discovered_from_game_scan": candidate.discovered_from_game_scan,
            "discovered_sources": list(candidate.discovered_sources),
            "auto_seeded_from_existing_tab": False,
            "updated_utc": datetime.datetime.utcnow().replace(microsecond=0).isoformat() + "Z",
        }

    for candidate in candidates:
        existing = ledger_items.get(candidate.name)
        if existing and _has_complete_unlock_review_decision(existing):
            existing["name"] = candidate.name
            existing["detected_category"] = candidate.detected_category
            existing["detected_subcategory"] = candidate.detected_subcategory
            existing["detected_order"] = candidate.detected_order
            existing["source_tab_key"] = candidate.source_tab_key
            existing["fingerprint"] = _candidate_fingerprint(candidate)
            existing["discovered_from_game_scan"] = candidate.discovered_from_game_scan
            existing["discovered_sources"] = list(candidate.discovered_sources)
            metadata_changed = True
            continue
        pending.append(candidate)

    if metadata_changed:
        _save_unlock_review_ledger(ledger_path, ledger_items)

    total_pending = len(pending)
    if total_pending == 0:
        print("[ok] unlock review: nothing pending (all discovered delta items are already categorized)")
        return 0, 0

    if max_items > 0 and len(pending) > max_items:
        pending = pending[:max_items]

    if not sys.stdin.isatty():
        print("[warn] unlock review requested in non-interactive mode; skipping prompts")
        return 0, total_pending

    reviewed_now = 0
    print(f"[ok] unlock review pending: {total_pending}")
    for idx, candidate in enumerate(pending, start=1):
        print("")
        print(f"[review {idx}/{len(pending)}] {candidate.name}")
        detected_sub = candidate.detected_subcategory or "(none)"
        print(
            "  detected: "
            f"category={candidate.detected_category}, "
            f"subcategory={detected_sub}, "
            f"order={candidate.detected_order}"
        )

        include = _prompt_yes_no("  include this item?", default=True)
        if include:
            default_category = _normalize_unlock_review_category(
                candidate.detected_category,
                fallback=UNLOCK_REVIEW_TOP_CATEGORY_ORDER[0],
            )
            category = _prompt_select_option(
                "  select category",
                UNLOCK_REVIEW_TOP_CATEGORY_ORDER,
                default=default_category,
            )

            sub_options = UNLOCK_REVIEW_SUBCATEGORY_OPTIONS.get(category, [])
            if sub_options:
                default_subcategory = _normalize_unlock_review_subcategory(
                    category,
                    candidate.detected_subcategory,
                    fallback="",
                )
                subcategory = _prompt_select_option(
                    "  select subcategory",
                    sub_options,
                    default=default_subcategory,
                )
            else:
                subcategory = ""

            default_order = _compute_unlock_review_next_order(ledger_items, category, subcategory)
            alphabetical_order = _compute_unlock_review_alphabetical_order(
                ledger_items,
                category,
                subcategory,
                candidate.name,
            )
            order = _prompt_positive_int(
                "  set order",
                default=default_order,
                alphabetical=alphabetical_order,
            )
        else:
            category = ""
            subcategory = ""
            order = 0

        ledger_items[candidate.name] = _build_record(
            candidate,
            include=include,
            category=category,
            subcategory=subcategory,
            order=order,
        )
        _save_unlock_review_ledger(ledger_path, ledger_items)
        reviewed_now += 1

    return reviewed_now, total_pending


def _next_sibling_grid(parent: ET.Element, anchor: ET.Element) -> ET.Element | None:
    found_anchor = False
    for child in list(parent):
        if child is anchor:
            found_anchor = True
            continue
        if not found_anchor:
            continue
        if child.tag == "grid":
            return child
    return None


def _find_unlock_review_page_grid(
    page_rect: ET.Element | None,
    subcategory: str,
) -> ET.Element | None:
    if page_rect is None:
        return None

    normalized_subcategory = (subcategory or "").strip()
    if normalized_subcategory:
        matching_text_keys = {
            text_key
            for text_key, sub in UNLOCK_REVIEW_SUBCATEGORY_BY_TEXT_KEY.items()
            if sub == normalized_subcategory
        }
        if matching_text_keys:
            for label in page_rect.findall("./label[@text_key]"):
                text_key = (label.attrib.get("text_key") or "").strip()
                if text_key not in matching_text_keys:
                    continue
                grid = _next_sibling_grid(page_rect, label)
                if grid is not None:
                    return grid

    return page_rect.find("./grid")


def _find_unlock_review_all_grid(
    all_page_rect: ET.Element | None,
    category: str,
) -> ET.Element | None:
    if all_page_rect is None:
        return None

    matching_tab_keys = {
        tab_key
        for tab_key, mapped_category in UNLOCK_REVIEW_TOP_CATEGORY_BY_TAB_KEY.items()
        if mapped_category == category
    }
    canonical_tab_key = _unlock_review_tab_key_for_category(category)
    if canonical_tab_key:
        matching_tab_keys.add(canonical_tab_key)

    for label in all_page_rect.findall("./label[@text_key]"):
        text_key = (label.attrib.get("text_key") or "").strip()
        if text_key not in matching_tab_keys:
            continue
        grid = _next_sibling_grid(all_page_rect, label)
        if grid is not None:
            return grid

    return None


def _grid_has_unlock_entry(grid: ET.Element, item_name: str) -> bool:
    item_key = _candidate_name_key(item_name)
    for entry in grid.findall("./entry[@name]"):
        if _candidate_name_key(entry.attrib.get("name", "")) == item_key:
            return True
    return False


def _build_unlock_entry_from_grid_template(grid: ET.Element, item_name: str) -> ET.Element | None:
    template_entry = grid.find("./entry")
    if template_entry is None:
        return None

    new_entry = copy.deepcopy(template_entry)
    new_entry.set("name", item_name)

    icon = resolve_item_icon(item_name)
    tooltip_key = TOOLTIP_OVERRIDES.get(item_name) or item_name

    for sprite in new_entry.findall(".//sprite[@name='itemIcon']"):
        sprite.set("sprite", icon.sprite)
        sprite.set("tooltip_key", tooltip_key)
        sprite.set("color", icon.tint or "")

    for fill in new_entry.findall(".//filledsprite[@name='background']"):
        fill.set("fill", f"{{cvar({item_name})}}")

    if icon.type_icon:
        for type_icon in new_entry.findall(".//sprite[@name='itemtypeicon']"):
            type_icon.set("sprite", icon.type_icon)
            type_icon.set("color", icon.type_tint or "")
    else:
        for parent in new_entry.iter():
            for child in list(parent):
                if child.tag == "sprite" and child.attrib.get("name") == "itemtypeicon":
                    parent.remove(child)

    return new_entry


def _sort_grid_entries_by_name(grid: ET.Element) -> None:
    entries = grid.findall("./entry[@name]")
    if len(entries) < 2:
        return
    ordered = sorted(entries, key=lambda elem: (elem.attrib.get("name") or "").lower())
    for entry in entries:
        grid.remove(entry)
    for entry in ordered:
        grid.append(entry)


def _ensure_grid_capacity(grid: ET.Element) -> None:
    """Grow unlock grids to fit entries without changing designed layout width.

    Cols and pos stay as authored in the purple book template (even left/right
    gaps). When entries exceed rows*cols, only rows increase so icons wrap
    within the existing horizontal bounds instead of expanding off-screen.
    """
    entries = grid.findall("./entry")
    if not entries:
        return

    count = len(entries)

    try:
        rows = int(grid.attrib.get("rows", "1"))
    except ValueError:
        rows = 1
    rows = max(1, rows)

    try:
        cols = int(grid.attrib.get("cols", "1"))
    except ValueError:
        cols = 1
    cols = max(1, cols)

    needed_rows = max(rows, math.ceil(count / cols))
    if needed_rows != rows:
        grid.set("rows", str(needed_rows))


def _apply_unlock_review_ledger_to_unlockables_tab(
    xml_text: str,
    ledger_items: dict[str, dict[str, typing.Any]],
    recipe_names: set[str],
    recipes_loaded: bool,
) -> tuple[str, int]:
    if not xml_text:
        return xml_text, 0

    try:
        root = ET.fromstring(xml_text)
    except ET.ParseError:
        return xml_text, 0

    unlock_rect = root.find(".//rect[@name='unlockablesTab']")
    if unlock_rect is None:
        return xml_text, 0

    tabs_contents = unlock_rect.find("./rect[@name='tabs']/rect[@name='tabsContents']")
    if tabs_contents is None:
        tabs_contents = unlock_rect.find("./rect[@name='tabsContents']")
    if tabs_contents is None:
        return xml_text, 0

    all_page_rect = tabs_contents.find("./rect[@tab_key='agf1MainTabOverview']")
    if all_page_rect is None:
        all_page_rect = tabs_contents.find("./rect[@tab_key='lblAll']")
    if all_page_rect is None:
        all_page_rect = tabs_contents.find("./rect[@name='unlockAll']")

    existing_name_keys = {
        _candidate_name_key((entry.attrib.get("name") or "").strip())
        for entry in unlock_rect.findall(".//entry[@name]")
        if (entry.attrib.get("name") or "").strip()
    }

    category_priority = {name: idx for idx, name in enumerate(UNLOCK_REVIEW_TOP_CATEGORY_ORDER)}
    candidate_rows: list[tuple[int, int, str, str, str]] = []
    for record in ledger_items.values():
        if _coerce_bool(record.get("include"), default=None) is not True:
            continue
        if not _has_complete_unlock_review_decision(record):
            continue

        name = (record.get("name") or "").strip()
        if not name:
            continue
        if _is_uncraftable_item_modifier(name, recipe_names, recipes_loaded):
            continue

        category = _normalize_unlock_review_category(str(record.get("category") or ""), fallback="")
        if not category:
            continue
        subcategory = _normalize_unlock_review_subcategory(
            category,
            str(record.get("subcategory") or ""),
            fallback="",
        )
        order = _coerce_positive_int(record.get("order"), default=99999)
        candidate_rows.append(
            (
                category_priority.get(category, 999),
                order,
                category,
                subcategory,
                name,
            )
        )

    if not candidate_rows:
        return xml_text, 0

    candidate_rows.sort(key=lambda row: (row[0], row[1], row[4].lower()))
    added_count = 0

    for _cat_sort, _order, category, subcategory, name in candidate_rows:
        name_key = _candidate_name_key(name)
        if name_key in existing_name_keys:
            continue

        category_tab_keys = {
            tab_key
            for tab_key, mapped_category in UNLOCK_REVIEW_TOP_CATEGORY_BY_TAB_KEY.items()
            if mapped_category == category
        }
        canonical_tab_key = _unlock_review_tab_key_for_category(category)
        if canonical_tab_key:
            category_tab_keys.add(canonical_tab_key)

        page_rect = None
        for page in tabs_contents.findall("./rect"):
            tab_key = (page.attrib.get("tab_key") or "").strip()
            if tab_key in category_tab_keys:
                page_rect = page
                break

        page_grid = _find_unlock_review_page_grid(page_rect, subcategory)
        all_grid = _find_unlock_review_all_grid(all_page_rect, category)

        added_this_item = False
        if page_grid is not None and not _grid_has_unlock_entry(page_grid, name):
            page_entry = _build_unlock_entry_from_grid_template(page_grid, name)
            if page_entry is not None:
                page_grid.append(page_entry)
                if category == "Drones":
                    _sort_grid_entries_by_name(page_grid)
                _ensure_grid_capacity(page_grid)
                added_this_item = True

        if all_grid is not None and not _grid_has_unlock_entry(all_grid, name):
            all_entry = _build_unlock_entry_from_grid_template(all_grid, name)
            if all_entry is not None:
                all_grid.append(all_entry)
                if category == "Drones":
                    _sort_grid_entries_by_name(all_grid)
                _ensure_grid_capacity(all_grid)
                added_this_item = True

        if added_this_item:
            existing_name_keys.add(name_key)
            added_count += 1

    if added_count == 0:
        return xml_text, 0

    return ET.tostring(root, encoding="unicode"), added_count


def _write_windows_backup(out_path: Path) -> Path | None:
    """Create timestamped backup of current windows.xml before overwrite."""
    if not out_path.exists():
        return None
    stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
    backup_dir = Path(__file__).resolve().parent / "_AUTOBACKUP"
    backup_dir.mkdir(parents=True, exist_ok=True)
    backup_path = backup_dir / f"windows.prewrite.{stamp}.xml"
    shutil.copy2(out_path, backup_path)
    return backup_path


def _prune_backup_files(backup_dir: Path, prefix: str, max_files: int) -> None:
    """Keep only the newest max_files backups for a given prefix."""
    if max_files <= 0 or not backup_dir.exists():
        return
    files = sorted(
        [p for p in backup_dir.glob(f"{prefix}*") if p.is_file()],
        key=lambda p: p.stat().st_mtime,
        reverse=True,
    )
    for stale in files[max_files:]:
        try:
            stale.unlink()
        except OSError:
            pass


def _write_localization_backup(loc_path: Path) -> Path | None:
    """Create timestamped backup of current Localization file before overwrite."""
    if not loc_path.exists():
        return None
    stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
    backup_dir = Path(__file__).resolve().parent / "_AUTOBACKUP"
    backup_dir.mkdir(parents=True, exist_ok=True)
    ext = loc_path.suffix or ".csv"
    backup_path = backup_dir / f"Localization.prewrite.{stamp}{ext}"
    shutil.copy2(loc_path, backup_path)
    return backup_path


def _latest_backup_file(backup_dir: Path, prefix: str) -> Path | None:
    """Return latest backup file matching prefix inside backup_dir."""
    if not backup_dir.exists():
        return None
    files = sorted(
        [p for p in backup_dir.glob(f"{prefix}*") if p.is_file()],
        key=lambda p: p.stat().st_mtime,
        reverse=True,
    )
    return files[0] if files else None


def _write_buffs_backup(buffs_path: Path) -> Path | None:
    """Create timestamped backup of current buffs.xml before overwrite."""
    if not buffs_path.exists():
        return None
    stamp = datetime.datetime.now().strftime("%Y%m%d-%H%M%S")
    backup_dir = Path(__file__).resolve().parent / "_AUTOBACKUP"
    backup_dir.mkdir(parents=True, exist_ok=True)
    backup_path = backup_dir / f"buffs.prewrite.{stamp}.xml"
    shutil.copy2(buffs_path, backup_path)
    return backup_path


def _skill_threshold_levels(skill: Skill) -> list[int]:
    """Return sorted unique unlock thresholds for a crafting skill."""
    levels: set[int] = set()
    for row_levels, _is_windowed in _skill_threshold_row_specs(skill):
        levels.update(row_levels)
    return sorted(levels)


def _skill_threshold_row_specs(skill: Skill) -> list[tuple[list[int], bool]]:
    """Return ordered row thresholds with a tiered-window flag per display row."""
    out: list[tuple[list[int], bool]] = []
    seen_global: set[int] = set()
    max_level = int(skill.max_level)

    for row in skill.display:
        row_levels: list[int] = []
        seen_row: set[int] = set()
        for level in row.unlock_levels:
            if not isinstance(level, int):
                continue
            if level < 1 or level > max_level:
                continue
            if level in seen_row or level in seen_global:
                continue
            seen_row.add(level)
            seen_global.add(level)
            row_levels.append(level)
        if row_levels:
            out.append((row_levels, bool(row.is_tiered)))

    return out


def _skill_threshold_rows(skill: Skill) -> list[list[int]]:
    """Return ordered unlock thresholds grouped by display row for one skill."""
    return [row_levels for row_levels, _is_windowed in _skill_threshold_row_specs(skill)]


def _extract_seed_recalc_book_effects(seed_buffs_text: str | None) -> list[str]:
    """Extract book/perk startup recalc rows from seed agfRecalculate book effect group."""
    if not (seed_buffs_text or "").strip():
        return []

    try:
        root = ET.fromstring(seed_buffs_text)
    except ET.ParseError:
        return []

    group = root.find(
        ".//buff[@name='agfRecalculate']/effect_group[@name='recaculateBookPorgression']"
    )
    if group is None:
        return []

    preserved: list[str] = []
    for te in group.findall("triggered_effect"):
        cvar = (te.attrib.get("cvar") or "").strip()
        if not cvar.startswith("AGF"):
            continue
        preserved.append(ET.tostring(te, encoding="unicode").strip())

    return preserved


def _extract_seed_book_runtime_append(seed_buffs_text: str | None) -> str | None:
    """Extract the AGF-only buffStatusCheck02 append block from seed buffs."""
    if not (seed_buffs_text or "").strip():
        return None

    try:
        root = ET.fromstring(seed_buffs_text)
    except ET.ParseError:
        return None

    for app in root.findall("append"):
        if app.attrib.get("xpath") != "buffs/buff[@name='buffStatusCheck02']":
            continue
        effects = app.findall(".//triggered_effect")
        cvars = [(te.attrib.get("cvar") or "").strip() for te in effects]
        cvars = [c for c in cvars if c]
        if cvars and all(c.startswith("AGF") for c in cvars):
            return ET.tostring(app, encoding="unicode").strip()

    return None


def _emit_crafting_recalc_start_effects(skills: list[Skill]) -> list[str]:
    """Emit startup recalc effects for crafting totals and threshold CVars."""
    lines: list[str] = [
        "\t\t\t<!-- Crafting Progress - Startup Rebuild -->",
        "\t\t\t<!-- Crafting Total CVars Reset -->",
    ]

    for skill in skills:
        total_cvar = f"{skill.name}Check"
        lines.append(
            f"\t\t\t<triggered_effect trigger=\"onSelfBuffStart\" action=\"ModifyCVar\" cvar=\"{total_cvar}\" operation=\"set\" value=\"0\"/>"
        )

    lines.append("")
    lines.append("\t\t\t<!-- Crafting Unlock Threshold CVars Reset -->")
    for skill in skills:
        total_cvar = f"{skill.name}Check"
        for row_levels in _skill_threshold_rows(skill):
            for level in row_levels:
                lines.append(
                    f"\t\t\t<triggered_effect trigger=\"onSelfBuffStart\" action=\"ModifyCVar\" cvar=\"{total_cvar}{level}\" operation=\"set\" value=\"0\"/>"
                )

    lines.append("")
    lines.append("\t\t\t<!-- Crafting Total CVars Seeded From ProgressionLevel -->")
    for skill in skills:
        total_cvar = f"{skill.name}Check"
        for level in range(1, int(skill.max_level) + 1):
            lines.append(
                f"\t\t\t<triggered_effect trigger=\"onSelfBuffStart\" action=\"ModifyCVar\" cvar=\"{total_cvar}\" operation=\"set\" value=\"{level}\"><requirement name=\"ProgressionLevel\" progression_name=\"{skill.name}\" operation=\"GTE\" value=\"{level}\"/></triggered_effect>"
            )

    return lines


def _emit_crafting_runtime_checker_append(skills: list[Skill]) -> list[str]:
    """Emit runtime crafting checker append for buffStatusCheck02."""
    lines: list[str] = [
        "<append xpath=\"buffs/buff[@name='buffStatusCheck02']\">",
        "\t<effect_group name=\"craftingChecker\">",
        "\t\t<!-- Crafting Progress - Runtime Delta Updates -->",
        "\t\t<!-- Crafting Unlock Threshold Runtime Update -->",
    ]

    for skill in skills:
        total_cvar = f"{skill.name}Check"
        for row_levels, is_windowed in _skill_threshold_row_specs(skill):
            for idx, level in enumerate(row_levels):
                threshold_cvar = f"{total_cvar}{level}"

                next_level = row_levels[idx + 1] if is_windowed and idx + 1 < len(row_levels) else None

                if next_level is not None:
                    lines.append(
                        f"\t\t<triggered_effect trigger=\"onSelfBuffUpdate\" action=\"ModifyCVar\" cvar=\"{threshold_cvar}\" operation=\"set\" value=\"1\"><requirement name=\"CVarCompare\" cvar=\"{total_cvar}\" operation=\"GTE\" value=\"{level}\"/><requirement name=\"CVarCompare\" cvar=\"{total_cvar}\" operation=\"LT\" value=\"{next_level}\"/><requirement name=\"CVarCompare\" cvar=\"{threshold_cvar}\" operation=\"LT\" value=\"1\"/></triggered_effect>"
                    )
                else:
                    lines.append(
                        f"\t\t<triggered_effect trigger=\"onSelfBuffUpdate\" action=\"ModifyCVar\" cvar=\"{threshold_cvar}\" operation=\"set\" value=\"1\"><requirement name=\"CVarCompare\" cvar=\"{total_cvar}\" operation=\"GTE\" value=\"{level}\"/><requirement name=\"CVarCompare\" cvar=\"{threshold_cvar}\" operation=\"LT\" value=\"1\"/></triggered_effect>"
                    )

                lines.append(
                    f"\t\t<triggered_effect trigger=\"onSelfBuffUpdate\" action=\"ModifyCVar\" cvar=\"{threshold_cvar}\" operation=\"set\" value=\"0\"><requirement name=\"CVarCompare\" cvar=\"{total_cvar}\" operation=\"LT\" value=\"{level}\"/><requirement name=\"CVarCompare\" cvar=\"{threshold_cvar}\" operation=\"GTE\" value=\"1\"/></triggered_effect>"
                )

                if next_level is not None:
                    lines.append(
                        f"\t\t<triggered_effect trigger=\"onSelfBuffUpdate\" action=\"ModifyCVar\" cvar=\"{threshold_cvar}\" operation=\"set\" value=\"0\"><requirement name=\"CVarCompare\" cvar=\"{total_cvar}\" operation=\"GTE\" value=\"{next_level}\"/><requirement name=\"CVarCompare\" cvar=\"{threshold_cvar}\" operation=\"GTE\" value=\"1\"/></triggered_effect>"
                    )

    lines.append("")
    lines.append("\t\t<!-- Crafting Total CVars Runtime Refresh -->")
    for skill in skills:
        total_cvar = f"{skill.name}Check"
        for level in range(1, int(skill.max_level) + 1):
            lines.append(
                f"\t\t<triggered_effect trigger=\"onSelfBuffUpdate\" action=\"ModifyCVar\" cvar=\"{total_cvar}\" operation=\"set\" value=\"{level}\"><requirement name=\"ProgressionLevel\" progression_name=\"{skill.name}\" operation=\"GTE\" value=\"{level}\"/></triggered_effect>"
            )

    lines.extend(["\t</effect_group>", "</append>", ""])
    return lines


def _emit_buffs_recalculate_patch(skills: list[Skill], seed_buffs_text: str | None = None) -> str:
    """Generate Purple Book buffs.xml with startup recalc + runtime checker blocks."""
    crafting_skills = [s for s in skills if s.name.startswith("crafting")]
    preserved_book_recalc_effects = _extract_seed_recalc_book_effects(seed_buffs_text)
    preserved_book_runtime_append = _extract_seed_book_runtime_append(seed_buffs_text)
    lines: list[str] = [
        "<AGF-HUDPlus>",
        "",
        "<append xpath=\"/buffs\">",
        "\t<buff name=\"agfRecalculate\" hidden=\"true\">",
        "\t\t<stack_type value=\"ignore\"/>",
        "\t\t<duration value=\"8\"/>",
        "",
        "\t\t<!-- Purple Book Runtime Recalculate Buff -->",
        "",
        "\t\t<effect_group name=\"recaculateChecklistPorgression\">",
    ]

    lines.extend(_emit_crafting_recalc_start_effects(crafting_skills))

    lines.extend(["\t\t</effect_group>", ""])

    if preserved_book_recalc_effects:
        lines.extend(
            [
                "\t\t<effect_group name=\"recaculateBookPorgression\">",
                "\t\t\t<!-- Book Progress - Startup Rebuild -->",
            ]
        )
        for te_xml in preserved_book_recalc_effects:
            lines.append(f"\t\t\t{te_xml}")
        lines.extend(["\t\t</effect_group>", ""])

    lines.extend(
        [
            "\t</buff>",
            "</append>",
            "",
        ]
    )

    if preserved_book_runtime_append:
        lines.append(preserved_book_runtime_append)
        lines.append("")

    lines.extend(_emit_crafting_runtime_checker_append(crafting_skills))

    lines.extend(
        [
            "</AGF-HUDPlus>",
            "",
        ]
    )
    return "\n".join(lines)


def _extract_buffs_progression_max_by_trigger(root: ET.Element, trigger_name: str) -> dict[str, int]:
    """Return max ProgressionLevel threshold written per crafting skill for a trigger."""
    out: dict[str, int] = {}
    for te in root.iter("triggered_effect"):
        if te.attrib.get("trigger") != trigger_name:
            continue
        if te.attrib.get("action") != "ModifyCVar":
            continue
        cvar = (te.attrib.get("cvar") or "").strip()
        if not cvar.startswith("crafting") or not cvar.endswith("Check"):
            continue

        req = te.find("requirement")
        if req is None:
            continue
        if req.attrib.get("name") != "ProgressionLevel":
            continue
        if req.attrib.get("operation") != "GTE":
            continue

        prog_name = (req.attrib.get("progression_name") or "").strip()
        if not prog_name.startswith("crafting"):
            continue

        try:
            req_value = int(str(req.attrib.get("value") or "0"))
        except ValueError:
            continue

        if req_value > out.get(prog_name, 0):
            out[prog_name] = req_value

    return out


def _extract_buffs_check_cvars(root: ET.Element) -> set[str]:
    """Return all crafting *Check and *CheckN CVars written in buffs.xml."""
    cvars: set[str] = set()
    for te in root.iter("triggered_effect"):
        if te.attrib.get("action") != "ModifyCVar":
            continue
        cvar = (te.attrib.get("cvar") or "").strip()
        if re.match(r"^crafting[A-Za-z]+Check\d*$", cvar):
            cvars.add(cvar)
    return cvars


def _extract_buffs_threshold_levels(root: ET.Element) -> dict[str, set[int]]:
    """Return threshold levels found per crafting skill from *CheckN CVars."""
    out: dict[str, set[int]] = {}
    for cvar in _extract_buffs_check_cvars(root):
        match = re.match(r"^(crafting[A-Za-z]+)Check(\d+)$", cvar)
        if not match:
            continue
        skill_name, level = match.group(1), int(match.group(2))
        out.setdefault(skill_name, set()).add(level)
    return out


def _validate_buffs_contract(buffs_text: str, skills: list[Skill], windows_text: str) -> list[str]:
    """Validate that buffs coverage matches progression skill caps and UI references."""
    if not (buffs_text or "").strip():
        return ["buffs.xml text is empty"]

    try:
        buffs_root = ET.fromstring(buffs_text)
    except ET.ParseError as exc:
        return [f"buffs.xml parse failed: {exc}"]

    expected_max = {
        skill.name: int(skill.max_level)
        for skill in skills
        if skill.name.startswith("crafting")
    }
    if not expected_max:
        return ["no crafting skills parsed from progression.xml"]

    check_cvars = _extract_buffs_check_cvars(buffs_root)
    threshold_levels = _extract_buffs_threshold_levels(buffs_root)
    start_max = _extract_buffs_progression_max_by_trigger(buffs_root, "onSelfBuffStart")
    update_max = _extract_buffs_progression_max_by_trigger(buffs_root, "onSelfBuffUpdate")

    issues: list[str] = []
    for skill_name in sorted(expected_max.keys()):
        max_level = expected_max[skill_name]
        total_cvar = f"{skill_name}Check"
        if total_cvar not in check_cvars:
            issues.append(f"{skill_name}: missing total CVar '{total_cvar}'")

        expected_thresholds = set(_skill_threshold_levels(next(s for s in skills if s.name == skill_name)))
        actual_thresholds = threshold_levels.get(skill_name, set())
        if actual_thresholds != expected_thresholds:
            missing = sorted(expected_thresholds - actual_thresholds)
            extra = sorted(actual_thresholds - expected_thresholds)
            if missing:
                issues.append(f"{skill_name}: missing thresholds {missing[:12]}")
            if extra:
                issues.append(f"{skill_name}: unexpected thresholds {extra[:12]}")

        start_seen = start_max.get(skill_name, 0)
        if start_seen < max_level:
            issues.append(
                f"{skill_name}: onSelfBuffStart max {start_seen} < progression max {max_level}"
            )

        update_seen = update_max.get(skill_name, 0)
        if update_seen < max_level:
            issues.append(
                f"{skill_name}: onSelfBuffUpdate max {update_seen} < progression max {max_level}"
            )

    win_tokens = set(re.findall(r"\bcrafting[A-Za-z]+Check\d*\b", windows_text or ""))
    missing_ui_cvars = sorted(win_tokens - check_cvars)
    if missing_ui_cvars:
        preview = ", ".join(missing_ui_cvars[:10])
        extra = "" if len(missing_ui_cvars) <= 10 else f" (+{len(missing_ui_cvars)-10} more)"
        issues.append(
            "windows.xml references missing buffs CVars: "
            f"{preview}{extra}"
        )

    return issues


def _emit_gameevents_spawn_recalc_patch() -> str:
    """Return gameevents patch text that triggers Purple Book recalc on key spawn flows."""
    sequences = [
        "game_first_spawn",
        "game_on_spawn",
        "game_on_respawn_none",
        "game_on_respawn_default",
        "game_on_respawn_injured",
        "game_on_respawn_permanent",
    ]
    lines = ["<AGF-HUDPlus>", ""]
    for seq in sequences:
        lines.extend(
            [
                f"\t<append xpath=\"/gameevents/action_sequence[@name='{seq}']\">",
                "",
                "\t\t<action class=\"AddBuff\">",
                "\t\t\t<property name=\"buff_name\" value=\"agfRecalculate\" />",
                "\t\t</action>",
                "\t</append>",
                "",
            ]
        )
    lines.append("</AGF-HUDPlus>")
    lines.append("")
    return "\n".join(lines)


def _validate_localization_preservation(
    existing_text: str,
    merged_text: str,
    active_armor_names: set[str] | None = None,
) -> list[str]:
    """Return keys whose existing rows were modified by merge logic."""
    _eh, ex_map, ex_order = _parse_loc_map(existing_text)
    _mh, merged_map, _mo = _parse_loc_map(merged_text)
    allowed_changes = {
        "xuiSchematics",
        "agf0PurpleBookButton",
        "agf0PurpleBookButtonTooltip",
        "agf1MainHeaderHint",
        "agf1MainTabArmors",
        "agf1MainTabBooks",
        "agf1MainTabMagazines",
        "agf1MainTabOverview",
        "agf1MainTabOverviewTooltip",
        "agf1MainTabUnlocks",
        "agf4UnlocksCategoryAmmo",
        "agf4UnlocksCategoryAmmoRow44",
        "agf4UnlocksCategoryAmmoRow762",
        "agf4UnlocksCategoryAmmoRow9mm",
        "agf4UnlocksCategoryAmmoRowArrows",
        "agf4UnlocksCategoryAmmoRowBolts",
        "agf4UnlocksCategoryAmmoRowJunkTurret",
        "agf4UnlocksCategoryAmmoRowRockets",
        "agf4UnlocksCategoryAmmoRowShotgun",
        "agf4UnlocksCategoryArmors",
        "agf4UnlocksCategoryArmorsBoots",
        "agf4UnlocksCategoryArmorsFittings",
        "agf4UnlocksCategoryArmorsGeneral",
        "agf4UnlocksCategoryArmorsHelmet",
        "agf4UnlocksCategoryArmorsMuffledConnectors",
        "agf4UnlocksCategoryArmorsPlating",
        "agf4UnlocksCategoryArmorsPockets",
        "agf4UnlocksCategoryDrone",
        "agf4UnlocksCategoryMisc",
        "agf4UnlocksCategoryToolsWeapons",
        "agf4UnlocksCategoryToolsWeaponsBarrels",
        "agf4UnlocksCategoryToolsWeaponsBlockDamage",
        "agf4UnlocksCategoryToolsWeaponsClub",
        "agf4UnlocksCategoryToolsWeaponsMotorTool",
        "agf4UnlocksCategoryToolsWeaponsOtherGun",
        "agf4UnlocksCategoryToolsWeaponsOtherMelee",
        "agf4UnlocksCategoryToolsWeaponsScopes",
        "agf4UnlocksCategoryToolsWeaponsShotgun",
        "agf4UnlocksCategoryToolsWeaponsSpecial",
        "agf4UnlocksCategoryVehicles",
        "agf5ArmorsRatingHeavy",
        "agf5ArmorsRatingLight",
        "agf5ArmorsRatingMedium",
        "agf5ArmorSetBonusTooltip",
    }
    changed: list[str] = []
    deprecated_localization_keys = {
        "checklistHovering",
        "checklistZoomArmor",
        "checklistZoomMagazine",
        "agfChecklist",
        "agfBooks",
        "agfUnlocks",
        "agfArmors",
        "agfOverview",
        "agfAllOverviewTooltip",
        "agfHeaderHint",
        "lblAll",
        "agfNerdOutfittDescription",
        "agfPrimitiveDescription",
        "agfStatMobility",
        "agfStatNoiseIncrease",
        "agfStatStaminaChangeOT",
        "agfArmorSamuraiOutfit",
        "agfArmorSantaHatHelmetDescription",
        "agfFitnessBarteringDescription",
        "poweredDoors",
        "agfPoweredDoors",
        "agfArmorAssassinSetBonus",
        "agfArmorAthleticSetBonus",
        "agfArmorBikerSetBonus",
        "agfArmorCommandoSetBonus",
        "agfArmorEnforcerSetBonus",
        "agfArmorFarmerSetBonus",
        "agfArmorLumberjackSetBonus",
        "agfArmorMinerSetBonus",
        "agfArmorNerdSetBonus",
        "agfArmorNomadSetBonus",
        "agfArmorPreacherSetBonus",
        "agfArmorPrimitiveSetBonus",
        "agfArmorRaiderSetBonus",
        "agfArmorRangerSetBonus",
        "agfArmorRogueSetBonus",
        "agfArmorScavengerSetBonus",
    }
    for key in ex_order:
        if _migrate_category_key(key) != key:
            continue
        if key in deprecated_localization_keys:
            continue
        if _is_nonactive_armor_localization_key(key, active_armor_names):
            continue
        if _is_setbonus_localization_key(key):
            continue
        if _is_generated_armor_piece_desc_key(key):
            continue
        if key in allowed_changes:
            continue
        ex_line = ex_map.get(key, "")
        merged_line = merged_map.get(key, "")
        if merged_line != ex_line:
            changed.append(key)
    return changed

# ---------------------------------------------------------------------------
# Main
# ---------------------------------------------------------------------------

def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--out-mod", default=DEFAULT_OUT_MOD)
    ap.add_argument("--progression", default=str(DEFAULT_PROGRESSION))
    ap.add_argument("--qualityinfo", default=str(DEFAULT_QUALITYINFO))
    ap.add_argument("--items", default=str(DEFAULT_ITEMS))
    ap.add_argument("--blocks", default=str(DEFAULT_BLOCKS))
    ap.add_argument("--recipes", default=str(DEFAULT_RECIPES))
    ap.add_argument("--buffs", default=str(DEFAULT_BUFFS))
    ap.add_argument("--game-localization", default=str(DEFAULT_GAME_LOCALIZATION))
    ap.add_argument("--seed-windows", default=str(DEFAULT_SEED_WINDOWS))
    ap.add_argument(
        "--seed-buffs",
        default=str(DEFAULT_RELEASE_BUFFS),
        help="Seed buffs.xml used to preserve non-crafting book/perk recalc effects",
    )
    ap.add_argument("--books-template", default=str(DEFAULT_BOOKS_TEMPLATE))
    ap.add_argument("--unlocks-template", default=str(DEFAULT_UNLOCKS_TEMPLATE))
    ap.add_argument(
        "--review-unlocks",
        action=argparse.BooleanOptionalAction,
        default=False,
        help="Run interactive unlock hardlist review prompts and persist decisions",
    )
    ap.add_argument(
        "--unlock-review-ledger",
        default=str(DEFAULT_UNLOCK_REVIEW_LEDGER),
        help="Path to persistent unlock review ledger JSON",
    )
    ap.add_argument(
        "--unlock-review-max-items",
        type=int,
        default=25,
        help="Maximum pending unlock review prompts to ask in one run (0 = no limit)",
    )
    ap.add_argument(
        "--unlock-review-summary",
        action=argparse.BooleanOptionalAction,
        default=False,
        help="Print game-discovered minus accounted unlock summary",
    )
    ap.add_argument(
        "--merge-seed-tabs",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Allow --seed-windows as fallback source for books/unlocks (default: enabled)",
    )
    ap.add_argument(
        "--sync-activebuild",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Copy generated windows.xml to ActiveBuild target (default: enabled)",
    )
    ap.add_argument(
        "--sync-game-mod",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Copy generated windows.xml to installed game mod target (default: enabled)",
    )
    ap.add_argument("--activebuild-windows", default=str(DEFAULT_ACTIVEBUILD_WINDOWS))
    ap.add_argument("--game-mod-windows", default=str(DEFAULT_GAME_MOD_WINDOWS))
    ap.add_argument(
        "--strict-preserve-tabs",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Fail generation if books/unlocks tabs regress or become placeholders (default: enabled)",
    )
    ap.add_argument(
        "--autobackup",
        action=argparse.BooleanOptionalAction,
        default=True,
        help="Write pre-write safety backups for windows/localization (default: enabled)",
    )
    ap.add_argument(
        "--autobackup-max-files",
        type=int,
        default=20,
        help="Max backup files to keep per backup type in Generator/_AUTOBACKUP",
    )
    args = ap.parse_args()

    prog_path = Path(args.progression)
    if not prog_path.exists():
        print(f"[err] progression.xml not found: {prog_path}")
        return 2

    qualityinfo_path = Path(args.qualityinfo)
    detected_quality_tiers = parse_quality_tier_count(qualityinfo_path)
    expected_quality_tiers = len(QCOLORS)
    if detected_quality_tiers is None:
        print(
            f"[warn] qualityinfo.xml not parsed; assuming {expected_quality_tiers} playable quality tiers "
            f"({qualityinfo_path})"
        )
        detected_quality_tiers = expected_quality_tiers
    else:
        print(
            f"[ok] parsed {detected_quality_tiers} playable quality tiers from {qualityinfo_path.name}"
        )

    if detected_quality_tiers != expected_quality_tiers:
        print("[err] quality tier count mismatch:")
        print(f"      - qualityinfo playable tiers: {detected_quality_tiers}")
        print(f"      - generator tier slots/colors: {expected_quality_tiers}")
        print("      update tier rendering assumptions before generation")
        return 6

    items_path = Path(args.items)
    blocks_path = Path(args.blocks)
    recipes_path = Path(args.recipes)
    buffs_source_path = Path(args.buffs)
    game_loc_path = _resolve_game_localization_path(Path(args.game_localization))
    game_loc_header = _read_localization_header(game_loc_path)
    english_index, lang_indices, loc_header_fields = _localization_schema(game_loc_header)
    global ITEM_ICONS, ARMOR_VALUE_SERIES_BY_ITEM, ARMOR_SETBONUS_NUMERIC_SERIES_BY_OUTFIT
    ITEM_ICONS = parse_items(items_path, tag="item")
    print(f"[ok] parsed {len(ITEM_ICONS)} items from {items_path.name}")
    ARMOR_VALUE_SERIES_BY_ITEM = parse_armor_value_series_by_item(items_path)
    if ARMOR_VALUE_SERIES_BY_ITEM:
        print(f"[ok] parsed armor value tier series from {items_path.name}")
    ARMOR_SETBONUS_NUMERIC_SERIES_BY_OUTFIT = parse_armor_setbonus_numeric_series_from_buffs(buffs_source_path)
    if ARMOR_SETBONUS_NUMERIC_SERIES_BY_OUTFIT:
        print(f"[ok] parsed armor set-bonus tier series from {buffs_source_path.name}")
    armor_setbonus_desc_rows = parse_armor_setbonus_desc_rows_from_game_localization(game_loc_path)
    if armor_setbonus_desc_rows:
        print(f"[ok] parsed armor set bonus descriptions from {game_loc_path.name}")
    armor_piece_desc_rows = parse_armor_piece_desc_rows_from_game_localization(game_loc_path)
    if armor_piece_desc_rows:
        print(f"[ok] parsed armor piece descriptions (without set bonus) from {game_loc_path.name}")
    armor_setbonus_from_desc: dict[str, str] = {}
    block_icons = parse_items(blocks_path, tag="block")
    # Block icons fill in for unlock entries that are blocks (e.g. tripwirepost,
    # ceilingLight01_player). Item entries take precedence when both exist.
    added = 0
    for nm, ic in block_icons.items():
        if nm not in ITEM_ICONS:
            ITEM_ICONS[nm] = ic
            added += 1
    print(f"[ok] parsed {len(block_icons)} blocks from {blocks_path.name} ({added} new icons)")

    recipe_names, recipes_loaded = parse_recipe_names(recipes_path)
    if recipes_loaded:
        print(f"[ok] parsed {len(recipe_names)} recipes from {recipes_path.name}")
    else:
        print(f"[warn] recipes.xml not parsed; uncraftable item-mod filters disabled ({recipes_path})")

    skills = parse_progression(prog_path)
    quality_row_issues = _validate_progression_quality_rows(skills, detected_quality_tiers)
    if quality_row_issues:
        print("[err] progression quality row check failed:")
        for issue in quality_row_issues[:20]:
            print(f"      - {issue}")
        if len(quality_row_issues) > 20:
            print(f"      - ... (+{len(quality_row_issues)-20} more)")
        print("      generation stopped to avoid tier-model regressions")
        return 6

    book_series = parse_book_series(prog_path)
    print(f"[ok] parsed {len(skills)} crafting_skill blocks from {prog_path.name}")
    print(f"[ok] parsed {len(book_series)} book groups from {prog_path.name}")
    for s in skills:
        rows = len(s.display)
        kind = "tiered" if all(r.is_tiered for r in s.display) else "mixed"
        print(f"     {s.name:<28} max={s.max_level:<3} rows={rows} [{kind}]")

    # Reorder TAB_ORDER by column height buckets:
    # - tiered-only columns: longest -> shortest
    # - non-tiered columns: shortest -> longest
    by_name = {s.name: s for s in skills}

    def _col_height(sk: Skill) -> int:
        h = 100
        if sk.name == "craftingSeeds":
            seeds_slots = _group_slots_by_unlock_level(_collect_non_tiered_slots(sk))
            for _lvl, items, _names in seeds_slots:
                h += _nontiered_height(max(1, len(items)))
            return h
        for row in sk.display:
            if row.has_quality and len(row.unlock_levels) == 6:
                h += max(120, 10 + 25 * max(1, len(row.items)) + 5)
            else:
                for i in range(len(row.unlock_levels)):
                    items_here = row.level_items[i] if i < len(row.level_items) else row.items
                    n = max(1, len(items_here))
                    if sk.name == "craftingFood" and n <= 2:
                        h += 30
                    else:
                        h += _nontiered_height(n)
        return h

    head: list[str] = []
    tail: list[str] = []
    for nm in TAB_ORDER:
        sk = by_name.get(nm)
        if sk and all(r.is_tiered for r in sk.display):
            head.append(nm)
        else:
            tail.append(nm)
    head.sort(key=lambda n: _col_height(by_name[n]) if n in by_name else 0, reverse=True)
    tail.sort(key=lambda n: _col_height(by_name[n]) if n in by_name else 0)
    TAB_ORDER[:] = head + tail
    print("[ok] reordered TAB_ORDER (tiered head desc, non-tiered tail asc)")
    if head:
        print("     tiered (desc):")
        for nm in head:
            if nm in by_name:
                print(f"       {nm:<26} h={_col_height(by_name[nm])}")
    if tail:
        print("     non-tiered (asc):")
    for nm in tail:
        if nm in by_name:
            print(f"       {nm:<26} h={_col_height(by_name[nm])}")

    out_mod_dir = _resolve_out_mod_dir(args.out_mod)
    if not out_mod_dir.exists():
        print(f"[err] target mod folder does not exist: {out_mod_dir}")
        print("      Create it first, or pass --out-mod with an absolute/relative path")
        print("      (default expected location: 00_DLL-Projects/<mod-name>)")
        return 2

    out_xui_dir = out_mod_dir / "Config" / "XUi_InGame"
    out_xui_dir.mkdir(parents=True, exist_ok=True)
    out_path = out_xui_dir / "windows.xml"

    seed_buffs_path = Path(args.seed_buffs)
    if not seed_buffs_path.exists() and DEFAULT_RELEASE_BUFFS.exists():
        seed_buffs_path = DEFAULT_RELEASE_BUFFS
    backup_dir = Path(__file__).resolve().parent / "_AUTOBACKUP"
    pre_recreate_seed = _latest_backup_file(backup_dir, "buffs.pre-recreate.source.")
    if not seed_buffs_path.exists() and pre_recreate_seed is not None:
        seed_buffs_path = pre_recreate_seed
    out_buffs_path = out_mod_dir / "Config" / "buffs.xml"
    if not seed_buffs_path.exists() and out_buffs_path.exists():
        seed_buffs_path = out_buffs_path
    seed_buffs_text = seed_buffs_path.read_text(encoding="utf-8") if seed_buffs_path.exists() else None
    if seed_buffs_text:
        print(f"[ok] loaded seed buffs for non-crafting recalc preservation: {seed_buffs_path}")
    else:
        print(f"[warn] no seed buffs found for non-crafting recalc preservation: {seed_buffs_path}")

    xml_text = emit_window(skills)
    existing_windows_text = out_path.read_text(encoding="utf-8") if out_path.exists() else None

    seed_path = Path(args.seed_windows)
    if not seed_path.exists() and DEFAULT_RELEASE_WINDOWS.exists():
        seed_path = DEFAULT_RELEASE_WINDOWS
    if not seed_path.exists():
        legacy_release = WORKSPACE / "03_ReleaseSource" / "AGF-HUDPlus-PurpleBook-v2.0.1" / "Config" / "XUi" / "windows.xml"
        if legacy_release.exists():
            seed_path = legacy_release
    seed_text = seed_path.read_text(encoding="utf-8") if seed_path.exists() else None

    books_template_path = Path(args.books_template)
    books_template_text = books_template_path.read_text(encoding="utf-8") if books_template_path.exists() else None
    unlocks_template_path = Path(args.unlocks_template)
    unlocks_template_text = unlocks_template_path.read_text(encoding="utf-8") if unlocks_template_path.exists() else None

    books_sources: list[tuple[str, str | None]] = [
        ("existing output", existing_windows_text),
        ("books template", books_template_text),
    ]
    unlocks_sources: list[tuple[str, str | None]] = [
        ("existing output", existing_windows_text),
        ("unlocks template", unlocks_template_text),
    ]
    if args.merge_seed_tabs:
        books_sources.append((f"seed windows ({seed_path.name})", seed_text))
        unlocks_sources.append((f"seed windows ({seed_path.name})", seed_text))

    def _books_validator(candidate: str) -> tuple[bool, str | None]:
        ok, problems = _validate_books_rect_against_vanilla(candidate, book_series)
        if ok:
            return True, None
        return False, "; ".join(problems[:2])

    xml_text, books_source, books_err = _merge_named_tab_rect_from_sources(
        xml_text,
        "booksTab",
        books_sources,
        validator=_books_validator,
    )
    xml_text, unlocks_source, unlocks_err = _merge_named_tab_rect_from_sources(
        xml_text,
        "unlockablesTab",
        unlocks_sources,
    )

    if books_source and unlocks_source:
        print(f"[ok] merged booksTab from {books_source}")
        print(f"[ok] merged unlockablesTab from {unlocks_source}")
    else:
        if not books_source:
            print(f"[warn] no non-placeholder booksTab source found ({books_err})")
        if not unlocks_source:
            print(f"[warn] no non-placeholder unlockablesTab source found ({unlocks_err})")

    ledger_path = Path(args.unlock_review_ledger)
    ledger_items = _load_unlock_review_ledger(ledger_path)
    xml_text, applied_from_ledger = _apply_unlock_review_ledger_to_unlockables_tab(
        xml_text,
        ledger_items,
        recipe_names,
        recipes_loaded,
    )
    if applied_from_ledger > 0:
        print(f"[ok] unlock review applied {applied_from_ledger} ledger item(s) into unlockablesTab UI")

    if args.review_unlocks or args.unlock_review_summary:
        unlocks_rect = _extract_named_rect(xml_text, "unlockablesTab") or ""
        accounted_candidates = _extract_accounted_unlock_review_candidates(
            unlocks_rect,
            recipe_names,
            recipes_loaded,
        )
        discovered_candidates = _discover_game_unlock_review_candidates(
            items_path,
            prog_path,
            recipes_path,
            recipe_names,
            recipes_loaded,
        )

        accounted_name_keys = {_candidate_name_key(c.name) for c in accounted_candidates}
        pending_candidates = [
            c for c in discovered_candidates
            if _candidate_name_key(c.name) not in accounted_name_keys
        ]

        print(
            "[ok] unlock review discovery summary: "
            f"game_discovered={len(discovered_candidates)}, "
            f"accounted_in_unlock_tab={len(accounted_candidates)}, "
            f"pending_delta={len(pending_candidates)}"
        )

        if args.unlock_review_summary or args.review_unlocks:
            preview_limit = 25
            if pending_candidates:
                preview = ", ".join(c.name for c in pending_candidates[:preview_limit])
                if len(pending_candidates) > preview_limit:
                    preview += f", ... (+{len(pending_candidates) - preview_limit} more)"
                print(f"[ok] unlock review pending delta preview: {preview}")
            else:
                print("[ok] unlock review pending delta preview: (none)")

        if args.review_unlocks:
            seeded = _seed_accounted_unlock_review_items(
                accounted_candidates,
                ledger_items,
                ledger_path,
            )
            if seeded > 0:
                print(f"[ok] unlock review auto-seeded {seeded} accounted unlock-tab items")

            reviewed_now, total_pending = _run_unlock_review_session(
                pending_candidates,
                ledger_items,
                ledger_path,
                max_items=max(0, int(args.unlock_review_max_items)),
            )
            if reviewed_now > 0:
                xml_text, applied_after_review = _apply_unlock_review_ledger_to_unlockables_tab(
                    xml_text,
                    ledger_items,
                    recipe_names,
                    recipes_loaded,
                )
                if applied_after_review > 0:
                    print(f"[ok] unlock review applied {applied_after_review} reviewed item(s) into unlockablesTab UI")
                remaining = max(0, total_pending - reviewed_now)
                print(
                    f"[ok] unlock review captured {reviewed_now} decisions "
                    f"({remaining} remaining for future runs)"
                )

    xml_text, armor_setbonus_loc, armors_source, armors_err = _merge_and_template_armors(
        xml_text,
        existing_windows_text,
        seed_text,
    )
    if armors_source:
        print(f"[ok] merged armorsTab from {armors_source}")
    else:
        print(f"[warn] no valid armorsTab source found ({armors_err})")

    active_armor_names = _active_armor_names_from_setbonus_map(armor_setbonus_loc)
    if active_armor_names:
        before_setbonus_desc = len(armor_setbonus_desc_rows)
        before_piece_desc = len(armor_piece_desc_rows)
        armor_setbonus_desc_rows = _filter_armor_rows_to_active_sets(armor_setbonus_desc_rows, active_armor_names)
        armor_piece_desc_rows = _filter_armor_rows_to_active_sets(armor_piece_desc_rows, active_armor_names)
        removed_rows = (before_setbonus_desc - len(armor_setbonus_desc_rows)) + (before_piece_desc - len(armor_piece_desc_rows))
        if removed_rows > 0:
            print(f"[ok] filtered {removed_rows} cosmetic/legacy armor localization rows not used by active armor sets")

        armor_setbonus_from_desc = derive_setbonus_rows_from_desc_rows(
            armor_setbonus_desc_rows,
            english_index=english_index,
        )
        if armor_setbonus_from_desc:
            print("[ok] derived armor set bonus 1/2 from full-set descriptions")
    else:
        print("[warn] no active armorsTab set-bonus keys found; skipping armor localization filtering")

    if armor_setbonus_from_desc:
        armor_setbonus_loc.update(armor_setbonus_from_desc)

    # Keep secondary "All" controls consistent across all four pages and
    # center Unlocks non-All tabs while preserving their size.
    xml_text = _normalize_shared_all_buttons_and_unlocks_tabs(xml_text)
    xml_text = _normalize_global_header_hint(xml_text)
    buffs_text = _emit_buffs_recalculate_patch(skills, seed_buffs_text)

    if args.strict_preserve_tabs:
        tab_issues = _validate_required_tabs(xml_text)
        armors_rect = _extract_named_rect(xml_text, "armorsTab")
        _arm_ok, armors_issues = _validate_armors_rect_zoomed_cards(armors_rect or "")
        if not _arm_ok:
            tab_issues.extend([f"armorsTab: {issue}" for issue in armors_issues[:8]])
        if tab_issues:
            print("[err] tab preservation check failed:")
            for issue in tab_issues:
                print(f"      - {issue}")
            print("      windows.xml was not written to avoid losing work")
            return 3

    buffs_contract_issues = _validate_buffs_contract(buffs_text, skills, xml_text)
    if buffs_contract_issues:
        print("[err] buffs contract check failed:")
        for issue in buffs_contract_issues[:20]:
            print(f"      - {issue}")
        if len(buffs_contract_issues) > 20:
            print(f"      - ... (+{len(buffs_contract_issues)-20} more)")
        print("      generated buffs.xml was rejected to avoid regression")
        return 5
    crafting_skill_count = sum(1 for s in skills if s.name.startswith("crafting"))
    print(f"[ok] buffs contract check passed ({crafting_skill_count} crafting categories)")

    if args.autobackup:
        backup_path = _write_windows_backup(out_path)
        if backup_path:
            print(f"[ok] wrote safety backup: {backup_path}")
            _prune_backup_files(backup_path.parent, "windows.prewrite.", args.autobackup_max_files)
    out_path.write_text(xml_text, encoding="utf-8")
    print(f"[ok] wrote {out_path}  ({len(xml_text):,} bytes)")

    if args.sync_activebuild:
        activebuild_path = Path(args.activebuild_windows)
        if activebuild_path.parent.parent.exists():
            activebuild_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(out_path, activebuild_path)
            print(f"[ok] synced ActiveBuild windows: {activebuild_path}")
        else:
            print(f"[warn] ActiveBuild target missing: {activebuild_path}")

    if args.sync_game_mod:
        game_mod_path = Path(args.game_mod_windows)
        if game_mod_path.parent.parent.exists():
            game_mod_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(out_path, game_mod_path)
            print(f"[ok] synced game mod windows: {game_mod_path}")
        else:
            print(f"[warn] game mod target missing: {game_mod_path}")

    # xui.xml registers the schematics window_group so the button can open it
    xui_path = out_xui_dir / "xui.xml"
    xui_text = (
        '<?xml version="1.0" encoding="UTF-8"?>\n'
        '<AGF-PurpleBookGenerator>\n'
        '\t<insertbefore xpath="/xui/window_group[@name=\'crafting\']">\n'
        f'\t\t<window_group name="schematics" open_backpack_on_open="false" close_compass_on_open="false" stack_panel_y_offset="{STACK_PANEL_Y_OFFSET}">\n'
        '\t\t\t<window name="Schematics"/>\n'
        '\t\t</window_group>\n'
        '\t</insertbefore>\n'
        '</AGF-PurpleBookGenerator>\n'
    )
    xui_path.write_text(xui_text, encoding="utf-8")
    print(f"[ok] wrote {xui_path}  ({len(xui_text):,} bytes)")

    if args.sync_activebuild:
        activebuild_xui_path = Path(args.activebuild_windows).parent / "xui.xml"
        if activebuild_xui_path.parent.parent.exists():
            activebuild_xui_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(xui_path, activebuild_xui_path)
            print(f"[ok] synced ActiveBuild xui: {activebuild_xui_path}")
        else:
            print(f"[warn] ActiveBuild xui target missing: {activebuild_xui_path}")

    if args.sync_game_mod:
        game_mod_xui_path = Path(args.game_mod_windows).parent / "xui.xml"
        if game_mod_xui_path.parent.parent.exists():
            game_mod_xui_path.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(xui_path, game_mod_xui_path)
            print(f"[ok] synced game mod xui: {game_mod_xui_path}")
        else:
            print(f"[warn] game mod xui target missing: {game_mod_xui_path}")

    # gameevents.xml triggers the runtime recalc buff on player spawn.
    gameevents_path = out_mod_dir / "Config" / "gameevents.xml"
    gameevents_text = _emit_gameevents_spawn_recalc_patch()
    gameevents_path.write_text(gameevents_text, encoding="utf-8")
    print(f"[ok] wrote {gameevents_path}  ({len(gameevents_text):,} bytes)")

    if args.sync_activebuild:
        activebuild_gameevents_path = Path(args.activebuild_windows).parent.parent / "gameevents.xml"
        if activebuild_gameevents_path.parent.exists():
            shutil.copy2(gameevents_path, activebuild_gameevents_path)
            print(f"[ok] synced ActiveBuild gameevents: {activebuild_gameevents_path}")
        else:
            print(f"[warn] ActiveBuild gameevents target missing: {activebuild_gameevents_path}")

    if args.sync_game_mod:
        game_mod_gameevents_path = Path(args.game_mod_windows).parent.parent / "gameevents.xml"
        if game_mod_gameevents_path.parent.exists():
            shutil.copy2(gameevents_path, game_mod_gameevents_path)
            print(f"[ok] synced game mod gameevents: {game_mod_gameevents_path}")
        else:
            print(f"[warn] game mod gameevents target missing: {game_mod_gameevents_path}")

    # buffs.xml carries runtime progression CVars used by card tracking.
    buffs_path = out_mod_dir / "Config" / "buffs.xml"
    buffs_path.parent.mkdir(parents=True, exist_ok=True)
    if args.autobackup:
        buffs_backup_path = _write_buffs_backup(buffs_path)
        if buffs_backup_path:
            print(f"[ok] wrote safety backup: {buffs_backup_path}")
            _prune_backup_files(buffs_backup_path.parent, "buffs.prewrite.", args.autobackup_max_files)
    buffs_path.write_text(buffs_text, encoding="utf-8")
    print(f"[ok] wrote {buffs_path}  ({len(buffs_text):,} bytes)")

    if args.sync_activebuild:
        activebuild_buffs_path = Path(args.activebuild_windows).parent.parent / "buffs.xml"
        if activebuild_buffs_path.parent.exists():
            shutil.copy2(buffs_path, activebuild_buffs_path)
            print(f"[ok] synced ActiveBuild buffs: {activebuild_buffs_path}")
        else:
            print(f"[warn] ActiveBuild buffs target missing: {activebuild_buffs_path}")

    if args.sync_game_mod:
        game_mod_buffs_path = Path(args.game_mod_windows).parent.parent / "buffs.xml"
        if game_mod_buffs_path.parent.exists():
            shutil.copy2(buffs_path, game_mod_buffs_path)
            print(f"[ok] synced game mod buffs: {game_mod_buffs_path}")
        else:
            print(f"[warn] game mod buffs target missing: {game_mod_buffs_path}")

    # Localization for tab captions and preserved rich key set
    loc_path = out_mod_dir / "Config" / "Localization.csv"
    ncols = len(loc_header_fields)
    static_loc_rows = [
        ("agf1MainTabMagazines", "", "", "", "", "Magazines"),
        ("agf1MainTabBooks", "", "", "", "", "Books"),
        ("agf1MainTabUnlocks", "", "", "", "", "Unlocks"),
        ("agf4UnlocksCategoryAmmo", "", "", "", "", "Ammo"),
        ("agf4UnlocksCategoryAmmoRow44", "", "", "", "", ".44 Ammo"),
        ("agf4UnlocksCategoryAmmoRow762", "", "", "", "", "7.62 Ammo"),
        ("agf4UnlocksCategoryAmmoRow9mm", "", "", "", "", "9mm Ammo"),
        ("agf4UnlocksCategoryAmmoRowArrows", "", "", "", "", "Arrows"),
        ("agf4UnlocksCategoryAmmoRowBolts", "", "", "", "", "Bolts"),
        ("agf4UnlocksCategoryAmmoRowJunkTurret", "", "", "", "", "Junk Turret Ammo"),
        ("agf4UnlocksCategoryAmmoRowRockets", "", "", "", "", "Rockets"),
        ("agf4UnlocksCategoryAmmoRowShotgun", "", "", "", "", "Shotgun Ammo"),
        ("agf4UnlocksCategoryArmors", "", "", "", "", "Armors"),
        ("agf4UnlocksCategoryArmorsBoots", "", "", "", "", "Boots"),
        ("agf4UnlocksCategoryArmorsFittings", "", "", "", "", "Fittings"),
        ("agf4UnlocksCategoryArmorsGeneral", "", "", "", "", "General"),
        ("agf4UnlocksCategoryArmorsHelmet", "", "", "", "", "Helmets"),
        ("agf4UnlocksCategoryArmorsMuffledConnectors", "", "", "", "", "Muffled Connectors"),
        ("agf4UnlocksCategoryArmorsPlating", "", "", "", "", "Platings"),
        ("agf4UnlocksCategoryArmorsPockets", "", "", "", "", "Pockets"),
        ("agf4UnlocksCategoryDrone", "", "", "", "", "Drones"),
        ("agf4UnlocksCategoryMisc", "", "", "", "", "Misc"),
        ("agf4UnlocksCategoryToolsWeapons", "", "", "", "", "Tools & Weapons"),
        ("agf4UnlocksCategoryToolsWeaponsBarrels", "", "", "", "", "Barrels"),
        ("agf4UnlocksCategoryToolsWeaponsBlockDamage", "", "", "", "", "Block Damage"),
        ("agf4UnlocksCategoryToolsWeaponsClub", "", "", "", "", "Clubs"),
        ("agf4UnlocksCategoryToolsWeaponsMotorTool", "", "", "", "", "Motor Tools"),
        ("agf4UnlocksCategoryToolsWeaponsOtherGun", "", "", "", "", "Ranged General"),
        ("agf4UnlocksCategoryToolsWeaponsOtherMelee", "", "", "", "", "Melee General"),
        ("agf4UnlocksCategoryToolsWeaponsScopes", "", "", "", "", "Scopes"),
        ("agf4UnlocksCategoryToolsWeaponsShotgun", "", "", "", "", "Shotguns"),
        ("agf4UnlocksCategoryToolsWeaponsSpecial", "", "", "", "", "Special"),
        ("agf4UnlocksCategoryVehicles", "", "", "", "", "Vehicles"),
        ("agf1MainTabArmors", "", "", "", "", "Armors"),
        ("agf5ArmorsRatingLight", "", "", "", "", "Light Armors"),
        ("agf5ArmorsRatingMedium", "", "", "", "", "Medium Armors"),
        ("agf5ArmorsRatingHeavy", "", "", "", "", "Heavy Armors"),
        ("agf1MainTabOverview", "", "", "", "", "Overview"),
        ("agf1MainTabOverviewTooltip", "", "", "", "", "Return to full page overview."),
        ("agf1MainHeaderHint", "", "", "", "", "Hover for details. Use tabs to zoom in."),
        ("agf5ArmorSetBonusTooltip", "", "", "", "", "Set Bonus"),
        ("xuiSchematics", "", "", "", "", "PURPLE BOOK"),
        ("agf0PurpleBookButton", "", "", "", "", "PURPLE BOOK"),
        ("agf0PurpleBookButtonTooltip", "", "", "", "", "Purple Book"),
        ("agf5ArmorsRatingStatPhysicalDamageResist", "ui_display", "Item stat", "", "", "Armor Rating"),
        ("agf5ArmorsRatingStatPhysicalDamageResistHeavy", "ui_display", "Item stat", "", "", "Heavy Armor Rating"),
        ("agf5ArmorsRatingStatPhysicalDamageResistLight", "ui_display", "Item stat", "", "", "Light Armor Rating"),
        ("agf5ArmorsRatingStatPhysicalDamageResistMedium", "ui_display", "Item stat", "", "", "Medium Armor Rating"),
    ]

    loc_rows: list[list[str]] = []
    for key, file_name, typ, used_in_main, no_translate, english in static_loc_rows:
        row = [""] * ncols
        row[0] = key
        if ncols > 1:
            row[1] = file_name
        if ncols > 2:
            row[2] = typ
        if ncols > 3:
            row[3] = used_in_main
        if ncols > 4:
            row[4] = no_translate
        if english_index < ncols:
            row[english_index] = english
        loc_rows.append(row)

    if armor_setbonus_loc:
        for k in sorted(armor_setbonus_loc.keys()):
            v = armor_setbonus_loc[k]
            row = [""] * ncols
            row[0] = k
            if english_index < ncols:
                row[english_index] = v
            loc_rows.append(row)

    if armor_setbonus_desc_rows:
        for k in sorted(armor_setbonus_desc_rows.keys()):
            line = armor_setbonus_desc_rows[k]
            parsed = next(csv.reader([line]))
            if len(parsed) < ncols:
                parsed += [""] * (ncols - len(parsed))
            elif len(parsed) > ncols:
                parsed = parsed[:ncols]
            loc_rows.append(parsed)

    if armor_piece_desc_rows:
        for k in sorted(armor_piece_desc_rows.keys()):
            line = armor_piece_desc_rows[k]
            parsed = next(csv.reader([line]))
            if len(parsed) < ncols:
                parsed += [""] * (ncols - len(parsed))
            elif len(parsed) > ncols:
                parsed = parsed[:ncols]
            loc_rows.append(parsed)

    # Fill missing translations for rows that currently only have English.
    for i, row in enumerate(loc_rows):
        english = row[english_index] if english_index < len(row) else ""
        if english and all(not row[idx] for idx in lang_indices.values()):
            loc_rows[i] = _fill_missing_translations(row, lang_indices, english)

    loc_header_line = _row_to_csv_line(loc_header_fields)
    generated_loc_text = loc_header_line + "\n"
    generated_loc_text += "\n".join(_row_to_csv_line(row) for row in loc_rows) + "\n"

    existing_loc_path = loc_path if loc_path.exists() else (out_mod_dir / "Config" / "Localization.txt")
    existing_loc_text = existing_loc_path.read_text(encoding="utf-8") if existing_loc_path.exists() else None
    fallback_loc_path = WORKSPACE / "03_ReleaseSource" / "AGF-HUDPlus-PurpleBook-v2.0.1" / "Config" / "Localization.csv"
    if not fallback_loc_path.exists():
        fallback_loc_path = WORKSPACE / "03_ReleaseSource" / "AGF-HUDPlus-PurpleBook-v2.0.1" / "Config" / "Localization.txt"
    fallback_loc_text = fallback_loc_path.read_text(encoding="utf-8") if fallback_loc_path.exists() else None
    loc_text = _merge_localization_text(
        generated_loc_text,
        existing_loc_text,
        fallback_loc_text,
        active_armor_names=active_armor_names,
    )

    if existing_loc_text is not None:
        changed_keys = _validate_localization_preservation(
            existing_loc_text,
            loc_text,
            active_armor_names=active_armor_names,
        )
        if changed_keys:
            print("[err] localization preservation check failed:")
            preview = ", ".join(changed_keys[:8])
            extra = "" if len(changed_keys) <= 8 else f" (+{len(changed_keys)-8} more)"
            print(f"      - existing keys changed: {preview}{extra}")
            print("      Localization.csv was not written to avoid losing edits")
            return 4

    loc_text = _normalize_localization_csv_style(loc_text)

    if args.autobackup:
        loc_backup = _write_localization_backup(existing_loc_path if existing_loc_path.exists() else loc_path)
        if loc_backup:
            print(f"[ok] wrote localization backup: {loc_backup}")
            _prune_backup_files(loc_backup.parent, "Localization.prewrite.", args.autobackup_max_files)

    loc_path.write_text(loc_text, encoding="utf-8")
    print(f"[ok] wrote {loc_path}  ({len(loc_text):,} bytes)")

    if args.sync_activebuild:
        activebuild_loc_path = Path(args.activebuild_windows).parent.parent / "Localization.csv"
        if activebuild_loc_path.parent.exists():
            shutil.copy2(loc_path, activebuild_loc_path)
            print(f"[ok] synced ActiveBuild localization: {activebuild_loc_path}")
        else:
            print(f"[warn] ActiveBuild localization target missing: {activebuild_loc_path}")

    if args.sync_game_mod:
        game_mod_loc_path = Path(args.game_mod_windows).parent.parent / "Localization.csv"
        if game_mod_loc_path.parent.exists():
            shutil.copy2(loc_path, game_mod_loc_path)
            print(f"[ok] synced game mod localization: {game_mod_loc_path}")
        else:
            print(f"[warn] game mod localization target missing: {game_mod_loc_path}")

    return 0

if __name__ == "__main__":
    sys.exit(main())

