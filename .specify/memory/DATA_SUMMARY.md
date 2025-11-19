# Data Structure Summary - Quick Reference

## 📊 What We Have

**3 Core Files (repeating yearly)**:

1. **DIS_COM_UDI_2025.txt** (Communes)
   - Maps 3,000+ communes to their water networks
   - Fields: INSEE code, commune name, network ID, network name, start date
   - Size: 3.9 MB

2. **DIS_PLV_2025.txt** (Sampling Events)  
   - 100,000+ sampling records for the year
   - Fields: Network ID, sampling date/time, conformity results (bacterio/chimique)
   - Includes upstream network dependencies (network A supplies network B)
   - Size: 105.6 MB

3. **DIS_RESULT_2025.txt** (Measurements)
   - 1,000,000+ individual parameter measurements
   - Fields: Parameter code/name, measured value, unit, quality limit, conformity
   - Each sampling has 10-100+ measurements
   - Size: 1.44 GB

**Data Available By**:
- **Year**: 2010-2025 (all available on data.gouv.fr)
- **Department**: Broken down into 95 dept files (avoiding huge single files)
- **Commune & Network**: Hierarchical - multiple networks per commune

---

## 🔍 Key Data Points

### Communes & Networks
```
INSEE: "01001"  →  Commune: "ABERGEMENT-CLEMENCIAT"
                 ├─ Network: "001000556" - "BDS ST DIDIER/CHALARONNE"
                 └─ Network: "001004303" - "L'ABERGEMENT-DE-VAREY DALIVOY"
```

### Sampling Event Structure
```
Reference: "00100143925"
Date: "2025-01-21" at "12h35"
Network: "001000003" (AMBRONAY)
Upstream: "001001304" supplies 100% of flow
Result: ✅ CONFORM (C=conform, N=non-conform)
- Bacteriological: C
- Chemical: C  
- Reference Bacterio: C
- Reference Chimique: C
```

### Parameter Measurement Structure
```
Parameter: "CHLORURE DE VINYL MONOMÈRE" (code: CLVYL)
Measured Value: 0.167 µg/L
Quality Limit: <=0.5 µg/L
Status: ✅ CONFORM
Type: Numeric (vs Qualitative like "Aspect normal")
```

---

## 🎯 What This Means for Our Models

### File Relationships
```
DIS_COM_UDI (communes) 
    ↓ (1:N)
DIS_PLV (sampling events)
    ↓ (1:N)
DIS_RESULT (measurements)
```

### Recommended Models to Create

1. **Commune**
   - INSEE code (PK)
   - Name
   - Networks collection

2. **WaterNetwork**
   - Network code (PK)
   - Name
   - Commune reference
   - Upstream network (if any)
   - Flow percentage from upstream

3. **SamplingEvent**
   - Reference ID (PK)
   - Network reference
   - Date & Time
   - Bacterio conformity (C/N)
   - Chemical conformity (C/N)
   - Overall conformity verdict
   - Conclusion text

4. **WaterParameter**
   - Parameter code (PK)
   - Major name
   - Minor name
   - Unit
   - Quality limit
   - Type (numeric vs qualitative)

5. **Measurement**
   - Reference ID (PK)
   - Sampling event reference
   - Parameter reference
   - Measured value
   - Conformity (C/N)
   - CAS number (for chemicals)

6. **ComplianceReport** (computed)
   - Network
   - Year/Month
   - Total samplings
   - Conform count
   - Non-conform count
   - Compliance percentage
   - Problem parameters list

---

## 💡 Business Logic Opportunities

### ComplianceChecker Service
- Compare `rqana` (measured value) against `limitequal`
- Mark conformity based on comparison
- Handle qualitative values (e.g., "SANS OBJET" = no limit)

### CommuneSearcher Service
- Search communes by INSEE code or name
- Get all networks for a commune
- Track upstream dependencies

### AnalysisGenerator Service
- Aggregate measurements into yearly reports
- Identify non-compliant parameters
- Generate risk scores
- Track trends (2024 vs 2025)

---

## ⚡ Data Loading Strategy

**Recommended order** (for MVP):
1. Load `DIS_COM_UDI_2025.txt` → Populate Commune & WaterNetwork tables
2. Load `DIS_PLV_2025_*.txt` by department → Populate SamplingEvent table
3. Load `DIS_RESULT_2025.txt` in chunks → Populate Measurement table

**Why department-by-department**: Parallel processing, memory efficiency, don't need 1.44 GB in memory at once

---

## 📈 Data Volume

| What | Count | Memory Impact |
|------|-------|----------------|
| Communes | 3,000+ | Small |
| Networks | 1,000+ | Small |
| Samplings/year | 100,000+ | Medium |
| Measurements/year | 1,000,000+ | Large - suggest pagination/batching |

---

## 🔗 Next Phase

Once models are created, we should:
1. Create data import service
2. Build ETL for CSV parsing
3. Validate conformity calculations
4. Create API endpoints for querying data
5. Build UI dashboards for compliance visualization

