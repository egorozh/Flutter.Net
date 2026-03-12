import 'package:flutter/material.dart';

import 'counter_widgets.dart';

class MaterialButtonsDemoPage extends StatefulWidget {
  const MaterialButtonsDemoPage({super.key});

  @override
  State<MaterialButtonsDemoPage> createState() =>
      _MaterialButtonsDemoPageState();
}

class _MaterialButtonsDemoPageState extends State<MaterialButtonsDemoPage> {
  bool _enabled = true;
  int _textButtonTaps = 0;
  int _elevatedButtonTaps = 0;
  int _outlinedButtonTaps = 0;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      spacing: 10,
      children: <Widget>[
        const Text(
          'Material buttons baseline',
          style: TextStyle(fontSize: 20, color: Colors.black),
        ),
        const Text(
          'TextButton / ElevatedButton / OutlinedButton with enabled/disabled and theme-aware defaults.',
          style: TextStyle(fontSize: 14, color: Colors.black54),
        ),
        Row(
          spacing: 8,
          children: <Widget>[
            _buildControlButton(
              label: _enabled ? 'Enabled' : 'Disabled',
              onTap: _toggleEnabled,
              width: 108,
              background: const Color(0xFFE9F0FF),
            ),
            _buildControlButton(
              label: 'Reset',
              onTap: _resetCounters,
              width: 88,
              background: const Color(0xFFF3E8D8),
            ),
          ],
        ),
        Text(
          'enabled=$_enabled, text=$_textButtonTaps, elevated=$_elevatedButtonTaps, outlined=$_outlinedButtonTaps',
          style: const TextStyle(fontSize: 12, color: Colors.blueGrey),
        ),
        SizedBox(
          width: 240,
          child: TextButton(
            onPressed: _enabled ? _onTextButtonTap : null,
            child: Text('TextButton taps: $_textButtonTaps'),
          ),
        ),
        SizedBox(
          width: 240,
          child: ElevatedButton(
            onPressed: _enabled ? _onElevatedButtonTap : null,
            child: Text('ElevatedButton taps: $_elevatedButtonTaps'),
          ),
        ),
        SizedBox(
          width: 240,
          child: OutlinedButton(
            onPressed: _enabled ? _onOutlinedButtonTap : null,
            child: Text('OutlinedButton taps: $_outlinedButtonTaps'),
          ),
        ),
        Row(
          spacing: 8,
          children: <Widget>[
            Expanded(
              child: ElevatedButton(
                onPressed: _enabled ? _onElevatedButtonTap : null,
                style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF6A994E),
                  foregroundColor: Colors.white,
                ),
                child: const Text('Custom elevated'),
              ),
            ),
            Expanded(
              child: OutlinedButton(
                onPressed: _enabled ? _onOutlinedButtonTap : null,
                style: OutlinedButton.styleFrom(
                  foregroundColor: const Color(0xFF7B2CBF),
                  side: const BorderSide(color: Color(0xFF7B2CBF), width: 1),
                ),
                child: const Text('Custom outlined'),
              ),
            ),
          ],
        ),
      ],
    );
  }

  Widget _buildControlButton({
    required String label,
    required VoidCallback onTap,
    required double width,
    required Color background,
  }) {
    return SizedBox(
      width: width,
      child: CounterTapButton(
        label: label,
        onTap: onTap,
        background: background,
        foreground: Colors.black,
        fontSize: 12,
        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
      ),
    );
  }

  void _toggleEnabled() {
    setState(() {
      _enabled = !_enabled;
    });
  }

  void _resetCounters() {
    setState(() {
      _textButtonTaps = 0;
      _elevatedButtonTaps = 0;
      _outlinedButtonTaps = 0;
      _enabled = true;
    });
  }

  void _onTextButtonTap() {
    setState(() {
      _textButtonTaps += 1;
    });
  }

  void _onElevatedButtonTap() {
    setState(() {
      _elevatedButtonTaps += 1;
    });
  }

  void _onOutlinedButtonTap() {
    setState(() {
      _outlinedButtonTaps += 1;
    });
  }
}
