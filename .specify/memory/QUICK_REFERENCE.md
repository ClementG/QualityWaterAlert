# 📌 Quick Reference Card - Water Quality Data

## File Formats At A Glance

### DIS_COM_UDI_2025.txt - Communes & Networks Mapping
```
📊 Purpose: Master list of communes and their water networks
📏 Size: ~3.9 MB
📋 Records: ~5,000 (communes can have 1-10+ networks)

Main Columns:
├─ inseecommune      → Commune ID ("01001")
├─ nomcommune        → Commune name ("ABERGEMENT-CLEMENCIAT")
├─ quartier          → Neighborhood ("-" if N/A)
├─ cdreseau          → Network code ("001000556")
├─ nomreseau         → Network name ("BDS ST DIDIER")
└─ debutalim         → Supply start date ("2010-09-07")

💡 Use: Find all networks for a commune
🔑 Primary Key: inseecommune + cdreseau
```

### DIS_PLV_2025.txt - Sampling Events
```
📊 Purpose: Water quality samplings with pass/fail verdicts
📏 Size: ~105.6 MB
📋 Records: ~100,000 per year (multiple per network)

Main Columns:
├─ cdreseau          → Network code
├─ referenceprel     → Sampling ID ("00100143925")
├─ dateprel          → Date ("2025-01-21")
├─ heureprel         → Time ("12h35")
├─ conclusionprel    → Text verdict
├─ plvconformitebacterio   → Bacterio result (C=Pass/N=Fail)
├─ plvconformitechimique   → Chemical result (C/N)
├─ cdreseauamont     → Upstream network if any
└─ pourcentdebit     → Flow % from upstream

💡 Use: Track compliance over time for a network
🔑 Primary Key: referenceprel
🔗 Links to: DIS_RESULT via referenceprel
```

### DIS_RESULT_2025.txt - Measurements
```
📊 Purpose: Individual parameter values (10-100+ per sampling)
📏 Size: ~1.44 GB
📋 Records: ~1,000,000+ per year

Main Columns:
├─ referenceprel              → Links to DIS_PLV
├─ cdparametresiseeaux        → Parameter code ("CLVYL")
├─ libmajparametre            → Parameter name
├─ qualitparam                → Type (N=numeric, O=qualitative)
├─ rqana                       → Measured value ("0.167" or "Normal")
├─ cdunitereference           → Unit ("µg/L")
├─ limitequal                 → Quality limit ("<=0.5 µg/L")
├─ valtraduite                → Numeric value for calculation
└─ casparam                   → CAS number if chemical

💡 Use: Detailed compliance analysis for specific parameters
🔑 Primary Key: referenceprel + cdparametre
🔗 Links to: DIS_PLV via referenceprel
```

---

## Data Types & Conversion Guide

### Conformity Status
```
'C'  → CONFORM (✅)      - All parameters pass
'N'  → NON-CONFORM (❌)  - One or more fail
''   → UNKNOWN (?)       - Not tested or unknown
```

### Parameter Quality Type
```
'N'  → Numeric      - Has measured value + unit (e.g., "0.167 µg/L")
'O'  → Qualitative  - Descriptive (e.g., "Aspect normal", "<1/100mL")
```

### Analysis Site
```
'L'  → Laboratory    - Professional lab analysis
'N'  → Field         - On-site/quick test
```

### Measured Value Formats
```
"0.167"         → Numeric value
"<1"            → Below detection (translate to 0 or min)
"Normal"        → Qualitative (no numeric value)
"SANS OBJET"    → Not applicable/no limit
""              → Missing/not tested
```

---

## Common Queries

### Find all networks in a commune
```
FROM DIS_COM_UDI
WHERE inseecommune = "01001"
SELECT DISTINCT cdreseau, nomreseau
```

### Get last 12 samplings for a network
```
FROM DIS_PLV
WHERE cdreseau = "001000556"
ORDER BY dateprel DESC
LIMIT 12
```

### Check conformity of network this year
```
FROM DIS_PLV
WHERE cdreseau = "001000556"
AND YEAR(dateprel) = 2025
GROUP BY plvconformitebacterio, plvconformitechimique
COUNT(*) as sample_count
```

### Find non-compliant measurements
```
FROM DIS_RESULT
WHERE cdparametre = "1753"  -- CHLORURE DE VINYL
AND CAST(valtraduite AS DECIMAL) > 0.5  -- Over limit of 0.5
```

### Which parameters are most tested
```
FROM DIS_RESULT
WHERE YEAR(dateprel) = 2025
GROUP BY cdparametresiseeaux
COUNT(*) as test_count
ORDER BY test_count DESC
```

---

## File Encoding & Format

```
Encoding:       UTF-8
Delimiter:      Comma (,)
Quote Char:     Double-quote (")
Escape:         Double-double quote ("") for literal quote
Line Ending:    LF or CRLF (handle both)
First Row:      Header row (column names)
```

### Example CSV Line
```
"001","00100143015","CLVYL","1753","CHLORURE DE VINYL MONOMÈRE","Chlorure de vinyl monomère","","N","L","0.167","µg/L","133","<=0.5 µg/L","","0.167000","75-01-4","00100152020"
```

---

## Data Import Checklist

- [ ] Download all 3 file types
- [ ] Verify UTF-8 encoding
- [ ] Check row counts
- [ ] Parse header row correctly
- [ ] Handle quoted values with commas
- [ ] Handle empty/missing values
- [ ] Convert dates from YYYY-MM-DD
- [ ] Convert times from "HHhmm" format
- [ ] Validate reference IDs match across files
- [ ] Test foreign key relationships
- [ ] Index on common query columns

---

## Performance Tips

### Indexes to Create
```sql
CREATE INDEX idx_commune ON Communes(INSEECode);
CREATE INDEX idx_network ON Networks(Code);
CREATE INDEX idx_sampling_date ON Samplings(SamplingDate DESC);
CREATE INDEX idx_sampling_network ON Samplings(NetworkCode, SamplingDate);
CREATE INDEX idx_measurement_ref ON Measurements(ReferencePrel);
CREATE INDEX idx_parameter ON Measurements(ParameterCode);
```

### Query Optimization
```
AVOID: Scanning entire DIS_RESULT (1.44 GB)
DO: Filter by date/network first, then join

AVOID: Loading all 1M+ measurements into memory
DO: Use pagination or batch processing

AVOID: Full-text search on parameter names
DO: Use parameter codes instead
```

### Recommended Batch Sizes
```
Communes: 1000-5000 per batch (small)
Samplings: 10,000 per batch (medium)
Measurements: 50,000-100,000 per batch (large file!)
```

---

## Department Codes (Quick Ref)

| Code | Department | Code | Department |
|------|------------|------|-----------|
| 001-019 | Nord-Ouest | 021-095 | Rest of France |
| 001 | Ain | 75 | Paris |
| 002 | Aisne | 76 | Seine-Maritime |
| 006 | Alpes-Maritimes | 91 | Essonne |
| 013 | Bouches-du-Rhône | 94 | Val-de-Marne |
| 014 | Calvados | 75 | Paris |

See `.specify/memory/` for full department mapping.

---

## Data Anomalies to Watch For

```
⚠️  Empty upstream network (cdreseauamont = "") 
    → Network is self-supplied, ignore upstream data

⚠️  Quartier = "-"
    → Entire commune, not specific neighborhood

⚠️  rqana = "SANS OBJET"
    → No quality limit for this parameter (always pass)

⚠️  Multiple entries for same commune/network/date
    → Different neighborhoods or supplier breaks (normal)

⚠️  valtraduite = "0.000000" with non-numeric rqana
    → Qualitative result (aspect/odor/taste)

⚠️  Year-over-year gaps
    → Some networks may not test every year
```

---

## Glossary

| Term | Meaning | Example |
|------|---------|---------|
| **UDP** | Unité De Distribution Publique | Public water network |
| **PLV** | Prélèvement | Sampling event |
| **UGE** | Unité Gestion Eau | Water management unit |
| **MOA** | Maître d'Ouvrage | Owner/Authority |
| **CAS** | Chemical Abstracts Service | "75-01-4" for Vinyl Chloride |
| **INSEE** | Institut National Statistique | 5-digit commune code |
| **SISE-eaux** | System information sanitaire eau | French water data system |

---

## Document Map

```
📄 EXECUTIVE_SUMMARY.md
   ↓ Overview of everything
   ├─ Quick facts & key insights
   ├─ Recommended models
   ├─ Data import strategy
   └─ Next steps

📄 DATA_SUMMARY.md (YOU ARE HERE)
   ↓ Quick reference & common tasks
   ├─ File formats
   ├─ Common queries
   ├─ Performance tips
   └─ Glossary

📄 DATA_STRUCTURE_ANALYSIS.md
   ↓ Deep technical dive
   ├─ Column-by-column breakdown
   ├─ Data characteristics
   ├─ Relationship model
   └─ Anomalies

📄 DATA_ARCHITECTURE_DIAGRAMS.md
   ↓ Visual relationships
   ├─ Data flow diagrams
   ├─ CSV structure examples
   ├─ Conformity logic
   └─ Aggregation examples
```

---

## Need Help?

1. **"What's in this file?"** → See File Formats section above
2. **"How do I find X?"** → See Common Queries section
3. **"How do I import this?"** → See Data Import Checklist
4. **"What are the relationships?"** → See DATA_ARCHITECTURE_DIAGRAMS.md
5. **"Detailed column info?"** → See DATA_STRUCTURE_ANALYSIS.md

