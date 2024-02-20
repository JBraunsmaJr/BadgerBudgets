# Badger Budgets

### Summary

One of many budgeting applications out there in the ether. The primary objective of this application was for me to
export financial related data from different sources then import them into an app where I can make sense of it all.

1. I didn't want third party software connecting to things.
2. Didn't want to upload/store things on some else's server
3. Ability to map columns from CSV files into columns the app understands/looks for.
4. Ability to apply transforms on data, or conditionally change a column based on value in another column.
5. Provide various data views, tables, and charts.

### Technology

Leverages Blazor WASM (Standalone). It **only runs in your browser**! 

No backend services.

Hosted via GitHub Pages because it's great for serving static content for free. However, note it's just to provide you
with the app itself. There's no cross-communication going on here with GitHub, or any external entity, and your client.

### Features

**Define Sources**:

We all have various financial institutions we're associated with. Heck, might have our own spreadsheet tracking things! 
There's a process for defining sources, and mapping columns from a sample to columns the application uses/understands.

Requires you specify:

- Transaction Date
- Amount
- Credit/Debit columns (whether they're separate, or appear in a single column)
- Category
- Description

----

**Ingest Data**:

Once you define a minimum of 1 source, you can start uploading your data!

----

**Transform Data**:

Sometimes data can look a bit rough. Wouldn't it be nice if we could automate cleaning it up a bit? Once data has been
ingested you'll be provided a table showcasing all the content. 

Double click on any cell to create a transform for that particular column, and to automatically grab the value from that 
cell. 

A transform can work one of two ways.

1. If value in column equals/has specified value -> change it to <insert your custom value here>
2. If value in column equals/has specified value AND the value in <specified column> equals/has <insert value> -> convert value to <insert your custom value here>

If a transform has been applied, you will see a tooltip which provides the original value.

----

**Filtering**

Basic filtering abilities for visualizing the data how you see fit