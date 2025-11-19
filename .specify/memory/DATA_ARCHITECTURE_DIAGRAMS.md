# Water Quality Data - Visual Architecture

## Data Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    FRENCH COMMUNES                              │
│                     (3,000+ entries)                            │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ INSEE Code: 01001                                        │  │
│  │ Name: ABERGEMENT-CLEMENCIAT                             │  │
│  │ Start Date: 2010-09-07                                  │  │
│  └──────────────────────────────────────────────────────────┘  │
│                           │                                     │
└───────────────────────────┼─────────────────────────────────────┘
                            │
                ┌───────────┴──────────┐
                │ (1:N relationship)   │
                ▼                      ▼
        ┌──────────────────────┬──────────────────────┐
        │  WATER NETWORKS      │   WATER NETWORKS     │
        │  (UDP 1)             │   (UDP 2)            │
        ├──────────────────────┼──────────────────────┤
        │ Code: 001000556      │ Code: 001004303      │
        │ Name: BDS ST DIDIER  │ Name: DALIVOY        │
        │ Commune: 01001       │ Commune: 01001       │
        │ Upstream: None       │ Upstream: None       │
        │ Flow: 100%           │ Flow: 100%           │
        └──────────────────────┴──────────────────────┘
                    │                    │
        ┌───────────▼──────────┬────────▼───────────┐
        │(1:N relationship)    │                     │
        ▼                      ▼                     ▼
    ┌─────────────────────┬─────────────────────┬─────────────────┐
    │  SAMPLING EVENT 1   │  SAMPLING EVENT 2   │  SAMPLING EVENT │
    │  (PLV)              │  (PLV)              │  (PLV)          │
    ├─────────────────────┼─────────────────────┼─────────────────┤
    │ Ref: 00100143925    │ Ref: 00100144152    │ Ref: 00100144537│
    │ Date: 2025-01-21    │ Date: 2025-02-12    │ Date: 2025-03-12│
    │ Time: 12h35         │ Time: 10h35         │ Time: 09h39     │
    │ Bacterio: C ✅      │ Bacterio: C ✅      │ Bacterio: C ✅  │
    │ Chemical: C ✅      │ Chemical: C ✅      │ Chemical: C ✅  │
    │ Upstream: TTP       │ Upstream: (none)    │ Upstream: (none)│
    │ Upstream Flow: 100% │                     │                 │
    └─────────────────────┴─────────────────────┴─────────────────┘
            │                   │                   │
    ┌───────▼────────┬──────────▼────────┬────────▼───────────┐
    │(1:N)           │(1:N)              │(1:N)               │
    ▼                ▼                   ▼                     ▼
  ┌────────────┐   ┌────────────┐    ┌─────────────┐   ┌─────────────┐
  │MEASUREMENT │   │MEASUREMENT │    │MEASUREMENT  │   │MEASUREMENT  │
  │(RESULT)    │   │(RESULT)    │    │(RESULT)     │   │(RESULT)     │
  ├────────────┤   ├────────────┤    ├─────────────┤   ├─────────────┤
  │Parameter:  │   │Parameter:  │    │Parameter:   │   │Parameter:   │
  │Chlorure    │   │Conductiv.  │    │Aspect       │   │E.Coli       │
  │Value:      │   │Value:      │    │Value:       │   │Value:       │
  │0.167 µg/L  │   │332 µS/cm   │    │Normal ✅    │   │<1/100mL     │
  │Limit:      │   │Limit:      │    │Limit: N/A   │   │Limit:       │
  │≤0.5 µg/L   │   │200-1100    │    │             │   │≤0/100mL     │
  │Status: ✅  │   │Status: ✅  │    │Status: ✅   │   │Status: ✅   │
  └────────────┘   └────────────┘    └─────────────┘   └─────────────┘
```

## CSV File Structure & Relationships

### File 1: DIS_COM_UDI_2025.txt
```
inseecommune  │ nomcommune              │ quartier          │ cdreseau   │ nomreseau              │ debutalim
──────────────┼─────────────────────────┼───────────────────┼────────────┼────────────────────────┼──────────────
01001         │ ABERGEMENT-CLEMENCIAT   │ -                 │ 001000556  │ BDS ST DIDIER...       │ 2010-09-07
01001         │ ABERGEMENT-CLEMENCIAT   │ Dalivoy les...    │ 001004303  │ L'ABERGEMENT-DE-VAREY  │ 2025-04-03
01002         │ ABERGEMENT-DE-VAREY     │ -                 │ 001000369  │ L'ABERGEMENT-DE-VAREY  │ 2010-09-07
```

**Purpose**: Master list of communes and their water distribution networks
**Key**: `inseecommune` + `cdreseau` forms the junction

### File 2: DIS_PLV_2025.txt
```
cddept │ cdreseau   │ inseecommune │ nomcommune  │ referenceprel  │ dateprel   │ plvconformitebacterio │ plvconformitechimique
───────┼────────────┼──────────────┼─────────────┼────────────────┼────────────┼───────────────────────┼──────────────────────
001    │ 001000003  │ 01007        │ AMBRONAY    │ 00100143925    │ 2025-01-21 │ C                     │ C
001    │ 001000003  │ 01007        │ AMBRONAY    │ 00100144152    │ 2025-02-12 │ C                     │ C
001    │ 001000003  │ 01007        │ AMBRONAY    │ 00100144537    │ 2025-03-12 │ C                     │ C
```

**Purpose**: Sampling events and their conformity verdicts
**Key**: `referenceprel` links to DIS_RESULT measurements

### File 3: DIS_RESULT_2025.txt
```
referenceprel  │ cdparametresiseeaux │ libmajparametre            │ qualitparam │ rqana │ cdunitereference │ limitequal    │ valtraduite
────────────────┼─────────────────────┼────────────────────────────┼─────────────┼───────┼──────────────────┼───────────────┼─────────────
00100143925    │ CLVYL               │ CHLORURE DE VINYL MONOMÈRE │ N           │ 0.167 │ µg/L             │ <=0.5 µg/L    │ 0.167
00100143925    │ ASP                 │ ASPECT (QUALITATIF)        │ O           │ Normal│ SANS OBJET       │ X             │ 0.0
00100143925    │ CDT25               │ CONDUCTIVITÉ À 25°C        │ N           │ 332   │ µS/cm            │ 200-1100      │ 332.0
```

**Purpose**: Individual parameter measurements for each sampling
**Key**: `referenceprel` links back to DIS_PLV

## Network Dependency Model

```
Network A (Main)
  │
  ├─ Commune 1 (100% local)
  │   └─ Sampling Events for A
  │       └─ Measurements
  │
  └─ Network B (Upstream)
      └─ Supplies 80% to Network A
      └─ Sampling Events for B
          └─ Measurements
```

Example:
```
Network: "SERA - SYNDICAT EAUX REGION D'AMBERIEU-EN-BUGEY" (001000003)
├─ Supply: 100% from own source → No upstream (cdreseauamont = "")
├─ or
├─ Supply: 60% from own source + 40% from "TTP (CLG) AMBRONAY" (001001304)
│   └─ Both have separate sampling records
```

## Conformity Logic

```
For each Sampling Event:
  ├─ Bacteriological Tests:
  │   ├─ E. Coli: <1/100mL
  │   ├─ Enterococci: <1/100mL
  │   ├─ Sulfito-reducers: <1/100mL
  │   └─ Result: C (conform) if all pass, else N (non-conform)
  │
  ├─ Chemical Tests:
  │   ├─ Heavy metals (lead, cadmium, etc.)
  │   ├─ Pesticides
  │   ├─ Disinfection byproducts
  │   └─ Result: C if all within limits, else N
  │
  ├─ Organoleptic:
  │   ├─ Aspect: Normal/Abnormal
  │   ├─ Taste/Odor: Normal/Abnormal
  │   └─ Result: C if normal, else N
  │
  └─ Final Conclusion: "Eau conforme aux exigences..." (C)
                    or "Eau non conforme..." (N)
```

## Data Aggregation Examples

### Report 1: Network Compliance by Month (2025)
```
Network: 001000003 (AMBRONAY)
Month    Samplings  Conform  Percentage  Status
────────┼──────────┼─────────┼───────────┼────────
Jan-25      3         3       100%       ✅
Feb-25      2         2       100%       ✅
Mar-25      3         3       100%       ✅
Apr-25      4         4       100%       ✅
─────────────────────────────────────────────────
YTD:       12        12       100%       ✅
```

### Report 2: Problem Parameters (2025)
```
Parameter: PLOMB (Lead)
Networks Affected: 45
Total Measurements: 1,234
Non-Conform: 12
Problem Rate: 0.97%
Threshold: ≤10 µg/L
Issues: Measurements 15-25 µg/L
```

### Report 3: Commune Risk Score
```
Commune: LYON
Networks: 8
Total Samplings: 128
Conform: 126 (98.4%)
Non-Conform: 2 (1.6%)
Risk Level: LOW ✅
Worst Parameter: Heavy Metals (1 non-conform)
```

## Technology Mapping

```
DIS_COM_UDI.txt ──┐
                   ├─► Database Tables ──┐
DIS_PLV.txt       │                      ├─► API Layer ──┐
                   │                      │               ├─► Web UI
DIS_RESULT.txt ──┘                      └─► Business     │
                                         Logic          └─► Reports
                                         (Compliance    
                                          Checker, etc)
```

