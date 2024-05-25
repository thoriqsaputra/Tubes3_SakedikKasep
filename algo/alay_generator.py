import random
import re

# Definisikan replacements
replacements = {
    'a': ['4', 'A', 'a'], 'b': ['B', 'b', '8', '13'], 'c': ['C', 'c'],
    'd': ['D', 'd'], 'e': ['3', 'E', 'e'], 'f': ['F', 'f'],
    'g': ['G', 'g', '6', '9'], 'h': ['H', 'h'], 'i': ['1', 'I', 'i', '!'],
    'j': ['J', 'j'], 'k': ['K', 'k'], 'l': ['L', 'l', '1'],
    'm': ['M', 'm'], 'n': ['N', 'n'], 
    'o': ['0', 'O', 'o'], 'p': ['P', 'p'], 'q': ['Q', 'q', '9'],
    'r': ['R', 'r', '12', 'i2', 'I2'], 's': ['S', 's', '5'], 't': ['T', 't', '7'],
    'u': ['U', 'u'], 'v': ['V', 'v'], 'w': ['W', 'w'],
    'x': ['X', 'x'], 'y': ['Y', 'y'], 'z': ['Z', 'z', '2']
}

# Inisialisasi reverse_replacements dan regex_pattern
reverse_replacements = {}
for key, values in replacements.items():
    for value in values:
        reverse_replacements[value.lower()] = key
reverse_replacements["12"] = "r"
reverse_replacements["i2"] = "r"
reverse_replacements["I2"] = "r"

sorted_keys = sorted(reverse_replacements.keys(), key=len, reverse=True)
regex_pattern = '|'.join(re.escape(key) for key in sorted_keys)

def alay_converter_advanced(full_name, use_number_symbol=True, use_case_mix=True, use_vowel_removal=False):
    modified_name = full_name.lower()
    
    if use_number_symbol:
        modified_name = ''.join(random.choice(replacements.get(char, [char])) for char in modified_name)
    
    if use_case_mix:
        modified_name = ''.join(random.choice([char.upper(), char.lower()]) for char in modified_name)
    
    if use_vowel_removal:
        vowels = 'aeiou'
        modified_name = ''.join(char for char in modified_name if char not in vowels and char.lower() not in vowels)
    
    return modified_name

def revert_alay_name(alay_name):
    def replace_match(match):
        key = match.group(0).lower()
        return reverse_replacements.get(key, match.group(0))
    
    modified_name = re.sub(regex_pattern, replace_match, alay_name, flags=re.IGNORECASE)
    modified_name = modified_name.lower().title()
    
    return modified_name

# Input dari pengguna
full_name = input("Masukkan nama lengkap: ")
random_alay_version = alay_converter_advanced(full_name, use_number_symbol=True, use_case_mix=True, use_vowel_removal=False)
print("Nama alay: " + random_alay_version)
print("Nama Revert: " + revert_alay_name(random_alay_version))
