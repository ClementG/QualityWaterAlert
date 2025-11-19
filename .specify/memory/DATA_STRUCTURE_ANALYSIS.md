# Water Quality Data Structure Analysis

## Overview

The French drinking water quality database from `data.gouv.fr` contains historical and current water quality control results organized by year and department. The data is provided in CSV format (pipe-delimited text files).

## Data Organization

### Folder Structure

```
datas/
├── dis-2025/                    # National consolidated data for 2025
│   ├── DIS_COM_UDI_2025.txt     # Communes/UDP mapping (3.9 MB)
│   ├── DIS_PLV_2025.txt         # Sampling points & results (105.6 MB)
│   └── DIS_RESULT_2025.txt      # Detailed measurement results (1.44 GB)
└── dis-2025-dept/              # Department-by-department breakdown
    ├── DIS_COM_UDI_2025.txt     # Communes/UDP mapping (shared)
    ├── DIS_PLV_2025_001.txt     # Ain (dept 001)
    ├── DIS_PLV_2025_002.txt     # Aisne (dept 002)
    ├── DIS_PLV_2025_003.txt     # Allier (dept 003)
    └── ... (95 departments total, missing 020 Corsica)
```

### File Types & Content

#### 1. DIS_COM_UDI_2025.txt - Communes & Water Distribution Units

**Purpose**: Maps communes to their water distribution networks (UDP - Unité De Distribution Publique)

**Structure**: CSV format with the following columns:

| Column | Type | Example | Meaning |
|--------|------|---------|---------|
| `inseecommune` | String | "01001" | INSEE code for the commune (5 digits) |
| `nomcommune` | String | "ABERGEMENT-CLEMENCIAT (L')" | Official commune name |
| `quartier` | String | "-" or "Dalivoy les granges" | Neighborhood/district or "-" if N/A |
| `cdreseau` | String | "001000556" | Network code (unique identifier for UDP) |
| `nomreseau` | String | "BDS ST DIDIER/CHALARONNE" | Network name |
| `debutalim` | Date | "2010-09-07" | Start date of water supply/operation |

**Key Insights**:
- One commune can have multiple networks (different quartiers or operators)
- The `-` value indicates commune-wide or undivided supply
- Dates range from 2010 onwards (historical data)

#### 2. DIS_PLV_2025.txt - Sampling Points & Analysis Conclusions

**Purpose**: Contains sampling event records (PLV = Prélèvement) with conformity verdicts and upstream network info

**Structure**: CSV format with key columns:

| Column | Type | Example | Meaning |
|--------|------|---------|---------|
| `cddept` | String | "001" | Department code (INSEE) |
| `cdreseau` | String | "001000003" | Network code |
| `inseecommuneprinc` | String | "01007" | Commune INSEE code |
| `nomcommuneprinc` | String | "AMBRONAY" | Commune name |
| `cdreseauamont` | String | "001001304" or "" | Upstream network code (if supplied by another network) |
| `nomreseauamont` | String | "TTP (CLG) AMBRONAY" or "" | Upstream network name |
| `pourcentdebit` | String | "100 %" | Flow percentage from upstream |
| `referenceprel` | String | "00100143925" | Sampling reference ID |
| `dateprel` | Date | "2025-01-21" | Sampling date |
| `heureprel` | String | "12h35" | Sampling time |
| `conclusionprel` | String | "Eau d'alimentation conforme..." | Conclusion text (conformity statement) |
| `ugelib` | String | "SYND. EAUX REGION D'AMBERIEU..." | Responsible UGE (management unit) |
| `distrlib` | String | "SERA - SYNDICAT..." | Distribution company/operator |
| `moalib` | String | "SERA - SYNDICAT..." | Master of work/owner |
| `plvconformitebacterio` | Char | "C" or "N" | Bacteriological conformity (C=conform, N=non-conform) |
| `plvconformitechimique` | Char | "C" or "N" | Chemical conformity |
| `plvconformitereferencebact` | Char | "C" or "N" | Reference bacteriological conformity |
| `plvconformitereferencechim` | Char | "C" or "N" | Reference chemical conformity |

**Key Insights**:
- Each row represents one sampling event for a specific network
- Multiple samples per network across the year
- Can track upstream dependencies (networks supplied by other networks)
- Conformity flags are boolean (C/N) or sometimes empty

#### 3. DIS_RESULT_2025.txt - Detailed Measurement Results

**Purpose**: Contains all individual parameter measurements from each sampling

**Structure**: CSV format with key columns:

| Column | Type | Example | Meaning |
|--------|------|---------|---------|
| `cddept` | String | "001" | Department code |
| `referenceprel` | String | "00100143015" | Links to sampling event in DIS_PLV |
| `cdparametresiseeaux` | String | "CLVYL" | Parameter code (SISE-eaux) |
| `cdparametre` | String | "1753" | Parameter numeric code |
| `libmajparametre` | String | "CHLORURE DE VINYL MONOMÈRE" | Major parameter name |
| `libminparametre` | String | "Chlorure de vinyl monomère" | Minor/detailed parameter name |
| `libwebparametre` | String | "" | Web-friendly parameter name |
| `qualitparam` | Char | "N" | Parameter quality flag (N=numeric, O=qualitative) |
| `insituana` | Char | "L" | Analysis site (L=laboratory, N=field) |
| `rqana` | String | "0.167" or "Aspect normal" | Actual measured value or qualitative result |
| `cdunitereferencesiseeaux` | String | "µg/L" | Unit (SISE-eaux) |
| `cdunitereference` | String | "133" | Unit code |
| `limitequal` | String | "<=0.5 µg/L" | Quality limit/reference |
| `refqual` | String | "" | Reference quality marker |
| `valtraduite` | String | "0.167000" | Numeric value (translated for calculation) |
| `casparam` | String | "" or "75-01-4" | CAS number (chemical identifier) |
| `referenceanl` | String | "00100152020" | Analysis reference |

**Key Insights**:
- One sampling can have 10-100+ parameter measurements
- Parameters are either numeric (measured with units) or qualitative (like "Aspect normal")
- Quality limits define compliance thresholds
- `valtraduite` is the standardized numeric value (even for comparative results like "<1")

## Data Characteristics

### Time Coverage
- **Available Years**: 2010-2025 (historical data)
- **Granularity**: Sampling events from 2010 to present
- **Update Frequency**: Annual releases with 2025 data fully loaded

### Geographic Coverage
- **Scope**: All French communes with drinking water distribution
- **Total Records**: 
  - ~3,000+ communes
  - ~1,000+ water networks
  - 100,000+ sampling events in 2025
  - 1M+ individual measurements in 2025

### Data Quality
- **Missing Values**: Indicated with empty strings or "-"
- **Encoding**: UTF-8
- **Format**: CSV with double-quote escaping
- **Delimiters**: Comma (,)

## Relationship Model

```
Commune (inseecommune)
    ├── Multiple Networks (cdreseau)
    │   └── Sampling Events (referenceprel) [in DIS_PLV]
    │       └── Measurements (per parameter) [in DIS_RESULT]
    │           └── Parameter Details
    │               ├── Value & Unit
    │               ├── Quality Limit
    │               └── Conformity
    └── Network Hierarchy
        └── Upstream Network (cdreseauamont)
            └── Percentage of flow supplied
```

## Key Parameters Categories

Based on the data, water quality parameters include:

1. **Microbiological**
   - Bacteriological indicators
   - E. coli presence
   - Enterococci

2. **Chemical**
   - Heavy metals (lead, cadmium, chromium, etc.)
   - Pesticides & plant protection products
   - Disinfection byproducts (chlorine, ozone residues)
   - Inorganic compounds (nitrates, phosphates)

3. **Physical**
   - Conductivity
   - pH
   - Turbidity
   - Temperature (sometimes)

4. **Organoleptic**
   - Aspect (visual)
   - Taste & odor
   - Color

## Recommended Data Import Strategy

### Phase 1: Commune & Network Setup
1. Load `DIS_COM_UDI_2025.txt`
2. Create Commune entities with associated networks
3. Track network hierarchy (upstream dependencies)

### Phase 2: Sampling Events
1. Load `DIS_PLV_2025.txt`
2. Create SamplingEvent records linked to networks
3. Store conformity conclusions

### Phase 3: Parameter Measurements
1. Load `DIS_RESULT_2025.txt`
2. Create Measurement records linked to sampling events
3. Parse values (handle both numeric and qualitative)
4. Store quality limits for compliance checking

### Phase 4: Historical Data (Optional)
1. Repeat for previous years (2024, 2023, etc.)
2. Can be imported incrementally

## Department Codes Reference

The `dis-2025-dept/` folder breaks down by French departments:
- 001-019: Metropolitan France (North-West)
- 021-095: Metropolitan France (excluding 020 = Corsica)
- Each file is named `DIS_PLV_2025_XXX.txt` where XXX is the dept code

This allows parallel processing or streaming import by department without loading all 1.44 GB at once.

## File Size Implications

| File | Size | Records | Avg Record |
|------|------|---------|-----------|
| DIS_COM_UDI | 3.9 MB | ~5,000 | ~800 bytes |
| DIS_PLV (national) | 105.6 MB | ~100,000 | ~1 KB |
| DIS_RESULT | 1.44 GB | ~1,000,000+ | ~1.5 KB |
| **Dept files** (each) | 400-4,300 KB | 2,000-40,000 | ~1 KB |

**Recommendation**: Load commune data first, then use department-by-department PLV files to avoid memory overhead when processing measurements.

## Next Steps for Model Creation

1. **Commune Model**: Basic commune + network aggregation
2. **WaterNetwork Model**: Network details, upstream dependencies
3. **SamplingEvent Model**: Date, time, conformity verdicts, upstream info
4. **Parameter Model**: Parameter definitions (code, name, unit, limits)
5. **Measurement Model**: Actual values, conformity, measurement details
6. **ComplianceAnalysis Model**: Aggregate conformity by time/location

