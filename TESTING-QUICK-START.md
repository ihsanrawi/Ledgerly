# 🚀 Quick Start: Test Duplicate Dialog

**5-Minute Test** - Get the duplicate dialog working ASAP!

---

## ✅ Prerequisites

- [x] Angular dev server running at `http://localhost:4200/` ✓
- [x] Build completed successfully ✓
- [x] Test CSV files created ✓

---

## 🎯 3-Step Quick Test

### Step 1: Start Backend API (if not running)

```bash
cd /home/ihsan/Desktop/Ledgerly/src/Ledgerly.Api
dotnet run
```

Wait for: `Now listening on: http://localhost:5000`

---

### Step 2: Import Baseline Data

1. Open browser: **http://localhost:4200/**
2. Navigate to **Import CSV** page
3. Upload: `/home/ihsan/Desktop/Ledgerly/tests/test-data/quick-test.csv`
4. Complete column mapping (should auto-detect)
5. **Import all transactions** (creates baseline)

---

### Step 3: Trigger Duplicate Dialog

1. Go back to **Import CSV** page
2. Upload **the same file again**: `quick-test.csv`
3. Complete column mapping
4. **🎉 Duplicate dialog should appear!**

---

## 🔍 What to Look For

**If it worked, you'll see:**

```
┌─────────────────────────────────────────┐
│  Duplicate Transactions Detected        │
│                                         │
│  [🟢 Exact Match]                       │
│                                         │
│  ┌──────────────┐  ┌──────────────┐   │
│  │  Existing    │  │  New         │   │
│  │  Coffee Shop │  │  Coffee Shop │   │
│  │  $4.50       │  │  $4.50       │   │
│  └──────────────┘  └──────────────┘   │
│                                         │
│  Reviewing duplicate 1 of 1             │
│                                         │
│  [Skip This] [Import Anyway]            │
└─────────────────────────────────────────┘
```

**Expected:**
- ✅ Two cards side-by-side
- ✅ Green badge "Exact Match"
- ✅ No yellow highlights (exact match)
- ✅ Blue border on left card (existing)
- ✅ Teal border on right card (new)
- ✅ Both show same data
- ✅ Action buttons at bottom

---

## 🎨 Test Different Scenarios

### Exact Match (Green Badge)
**File:** `quick-test.csv` row 1-2
- Same date, payee, amount, category, account
- Should show green badge, no highlights

### Likely Match (Yellow Badge)
**File:** `duplicate-test.csv` row 3-4 (Walmart)
- Same date, payee, amount, account
- Different category only
- Should show yellow badge, yellow highlight on category

### Possible Match (Gray Badge)
**File:** `duplicate-test.csv` row 5-6 (Starbucks)
- Same date, payee, category, account
- Different amount ($5.50 vs $5.49)
- Should show gray badge, yellow highlight on amount

---

## 🐛 Troubleshooting

### "No duplicate dialog appears"

**Check:**
1. Did you import the CSV once first? (creates baseline)
2. Is backend running? `curl http://localhost:5000/api/balance`
3. Check browser console (F12) for errors

**Fix:**
```bash
# Restart backend
cd /home/ihsan/Desktop/Ledgerly/src/Ledgerly.Api
dotnet run
```

---

### "Cards look wrong"

**Check:**
1. Browser console (F12) → Any errors?
2. CSS loading? Hard refresh (Ctrl+Shift+R)
3. Browser width > 768px? (for side-by-side)

**Fix:**
- Clear browser cache
- Restart dev server: Stop & run `ng serve`

---

### "Getting TypeScript errors"

**You shouldn't see any!** Build completed successfully.

If you do:
```bash
cd /home/ihsan/Desktop/Ledgerly/src/Ledgerly.Web
npm run build
```

Should complete without errors.

---

## 📸 Take Screenshots

Once working, capture:

1. **Desktop view** - Side-by-side cards with green badge
2. **Mobile view** - Stacked cards (resize to 375px)
3. **Yellow highlights** - Upload `duplicate-test.csv` twice, see Walmart difference
4. **Dark mode** - DevTools → Rendering → prefers-color-scheme: dark

Save to: `.ai/screenshots/after/`

---

## 📋 Full Test Files

**Quick test** (1 duplicate):
- `tests/test-data/quick-test.csv`

**Basic scenarios** (4 duplicates):
- `tests/test-data/duplicate-test.csv`

**Comprehensive** (5 duplicates + edge cases):
- `tests/test-data/duplicate-scenarios.csv`

**Full guide:**
- `tests/test-data/README-DUPLICATE-TESTING.md`

---

## ✅ Success Checklist

After testing, you should have verified:

- [ ] Side-by-side layout works
- [ ] Green badge for exact matches
- [ ] Yellow badge for likely matches
- [ ] Gray badge for possible matches
- [ ] Yellow highlights on different fields
- [ ] "Skip This" button works
- [ ] "Import Anyway" button works
- [ ] Navigation works (if multiple duplicates)
- [ ] Mobile responsive (stacks on small screens)
- [ ] Keyboard navigation (Tab, Enter)
- [ ] No console errors

---

## 🚀 Next Steps

**After testing passes:**

1. ✅ Mark "Test duplicate dialog" as complete
2. ✅ Take screenshots for documentation
3. ✅ Move to next task: **Virtualized scrolling** (2.5h estimate)

**If issues found:**
- Document in `.ai/bug-report.md`
- Let me know and I'll help fix them!

---

## 🎉 You're Ready!

**The dev server is running and test files are ready.**

Open your browser to: **http://localhost:4200/**

Good luck testing! If you see the side-by-side comparison with colored badges, it's working! 🚀
